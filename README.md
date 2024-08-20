Extensions for [BlishHUD](https://github.com/blish-hud/Blish-HUD)

### Instructions

Preferred way to use is as a *git submodule* to avoid dependency conflicts with other modules.
As a *git submodule* files under ``./Resources/*`` will have to be 
included in your project with their *Build Action* set to *Embedded Resource* (see below).

If you locate your submodules outside of your project directory you can include this project
by manually adding the following ItemGroup to your \*.csproj and adjusting as needed.

```xml
<ItemGroup>
    <Compile Include="..\submodules\bhud-extended\**\*.cs" Link="_Submodules\%(RecursiveDir)%(Filename)%(Extension)"/>
    <EmbeddedResource Include="..\submodules\bhud-extended\**\Properties\*.resx" Link="_Submodules\%(RecursiveDir)%(Filename)%(Extension)">
        <!--Required to make the ResourceManager find translations -->
        <LogicalName>Blish_HUD.Extended.Properties.%(Filename).resources</LogicalName>
        <Visible>False</Visible>
    </EmbeddedResource>
    <!-- Exclude AssemblyInfo.cs of submodule projects to avoid duplication error (Compile Error CS0579) -->
    <Compile Remove="..\submodules\**\AssemblyInfo.cs" />
</ItemGroup>
```
