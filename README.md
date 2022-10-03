Extensions for [BlishHUD](https://github.com/blish-hud/Blish-HUD)

### Instructions

Preferred way to use is as a *git submodule* to avoid dependency conflicts with other modules.
As a *git submodule* files under ``./Resources/*`` will have to be 
included in your project with their *Build Action* set to *Embedded Resource* (see below).

If you locate your submodules outside of your project directory you can include this project
by manually adding the following ItemGroup to your \*.csproj and adjusting as needed.

```xml
<ItemGroup>
  <Compile Include="..\submodules\bhud-extended\**\*.cs" Link="_Submodules\%(RecursiveDir)%(Filename)%(Extension)" />
  <!-- Exclude AssemblyInfo.cs of submodule projects to avoid duplication error (Compile Error CS0579) -->
  <Compile Remove="..\submodules\**\AssemblyInfo.cs"/>
  <EmbeddedResource Include="..\submodules\bhud-extended\**\Resources\*.png" Link="_Submodules\%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```
