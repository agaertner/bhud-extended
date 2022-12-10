using Gw2Sharp.Models;
using System;
using System.Threading.Tasks;

namespace Blish_HUD.Extended
{
    public static class PositionUtil
    {
        private const double JumpHeight = 1.622;
        private const double JumpHeightError = JumpHeight * 0.2;
        private const double MinJumpHeight = JumpHeight - JumpHeightError;
        private const double MaxJumpHeight = JumpHeight + JumpHeightError;
        private const double MaxHeightChange = 0.1f;
        private const double MaxWiggleDistanceSqr = 0.005;
        private const double MaxYSlide = 0.004;
        private static readonly TimeSpan JumpTime = TimeSpan.FromSeconds(0.65);
        private static readonly TimeSpan JumpTimeError = TimeSpan.FromSeconds(0.25);
        private static readonly TimeSpan MinJumpTime = JumpTime - JumpTimeError;
        private static readonly TimeSpan MaxJumpTime = JumpTime + JumpTimeError;
        private static readonly TimeSpan MaxStillTime = TimeSpan.FromSeconds(0.1);

        public static void RequestGroundPosition(Action<bool, Coordinates3> callback)
        {
            var thread = Task.Run(() =>
            {
                var startTime = DateTime.UtcNow;
                var endTime = DateTime.UtcNow;

                double maxDistanceXZSqr = 0;
                double maxGoneUp = 0;
                double maxGoneDown = 0;

                var startPos = GameService.Gw2Mumble.RawClient.AvatarPosition;
                var pos = GameService.Gw2Mumble.RawClient.AvatarPosition;

                // Only Upward movement
                while (pos.X == startPos.X && pos.Z == startPos.Z && pos.Y >= startPos.Y)
                {
                    var prevPos = pos;
                    pos = GameService.Gw2Mumble.RawClient.AvatarPosition;

                    maxDistanceXZSqr = Math.Max(maxDistanceXZSqr, Math.Pow(pos.X - startPos.X, 2) + Math.Pow(pos.Z - startPos.Z, 2));
                    maxGoneUp = Math.Max(maxGoneUp, pos.Y - startPos.Y);
                    maxGoneDown = Math.Min(maxGoneUp, pos.Y - startPos.Y);

                    // Moved too much
                    if (maxDistanceXZSqr > MaxWiggleDistanceSqr || maxGoneUp > MaxJumpHeight || maxGoneDown < -MaxHeightChange)
                    {
                        callback(false, pos);
                        return;
                    }

                    // Only update ended time if position is not close enough
                    if (!CloseEnough(prevPos, pos))
                    {
                        endTime = DateTime.UtcNow;
                    }

                    var taken = endTime - startTime;
                    // Took too long, byebye
                    if (taken > MaxJumpTime)
                    {
                        callback(false, pos);
                        return;
                    }

                    // Stood still for long enough...
                    if (DateTime.UtcNow - endTime >= MaxStillTime)
                    {
                        // Make all remaining tests...
                        var heightChange = Math.Abs(pos.Y - startPos.Y);
                        if (taken < MinJumpTime && maxGoneUp >= MinJumpHeight && heightChange <= MaxHeightChange)
                        {
                            callback(true, pos);
                            return;
                        }
                    }
                } 
            });
        }

        private static bool CloseEnough(Coordinates3 a, Coordinates3 b) => a.X == b.X && a.Z == b.Z && Math.Abs(a.Y - b.Y) < MaxYSlide;
    }
}
