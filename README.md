
# ![beetleeyes](https://old.thunderstore.io/thumbnail-serve/repository/icons/CoomGod-Beetleyes-1.0.0.png/?width=64&height=64)  BeetleEyes 
Just a silly mod i made for [BeetleBall](https://store.steampowered.com/app/2776270/Beetleball/), it adds some eyes to every beetle on the match


## Building from source

So like i made this mod without really thinking of making it future proof to build again so youll probably have to rewrite all the paths to dlls on the .csproj. I made this on linux and for some reason the melonloader team decided to make the wizard for making melonloader projects on Visual Studio which works exclusively on Windows, so i had to make a tool for wiring all the extracted dlls from the game.

Truth be told i dont use every single reference so you probably can delete 90% of them and leave the melonloader one and a few more.

## Usage
Other than that just make sure that the assets are properly installed to work
   ```
   Beetleball/
   └── Mods/
       ├── Beetleyes.dll
       └── assets/
           └── point.png
   ```
