# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.1]
 - Initial version
 
## [0.0.2] 
 - Rename to "Lion Auto Builder"
  
## [0.0.3] 
 - Rename namespace
 - Set back iOS build folder to builds/ios/<buildName>
 
## [0.0.4]
 - Set back iOS build folder to builds/ios/<environment>/<buildName> 

## [0.0.5]
 - Fix BuildSettings assets being reset when rebuilding Library

## [0.0.6]
 - Fix build symbols addition optional when values are null

## [0.0.7]
 - Fix manual builds

## [0.0.8]
- Remove additional define symbols
- External FakeCMDArgsProvider settings
- Remove jdkPath argument
- Remove Create menus for settings

## [0.0.9]
- Removed redundant properties
- Changed settings assets auto creation logic

## [0.0.10]
- create parent settings directory fix

## [0.0.12]
- Automatically create export.plist during build
- Remove oneSignalProductIdentifier setting, it's found automatically
- Move build menu items to LionStudios menu
