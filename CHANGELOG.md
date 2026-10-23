# Changelog

All notable changes to this project will be documented in this file.
## 1.3.7
- Added special check for particle previews.
- Added the header from the preview window the PreviewField so it has the same controls.

## 1.3.6
- Fixed a null ref error when deleting some prefab that would have been shown in a list.

## 1.3.5
- Fixed a null warning from failure to create a default empty texture when drawing nothing.

## 1.3.4
 - Fixed a reintroduced GUI.DrawTexture() null warning from 1.3.3.

## 1.3.3
- Fixed an issue where the scroll list position would not properly update its position, and the selected item could end up outside the viewable window.

## 1.3.2
- Fixed another GUI.DrawTexture() null warning.

## 1.3.1
- Fixed GUI.DrawTexture() null warning.

## 1.3.0
- Added new properties to the PreviewField attribute.
- Defaults to showing a PreviewImage in the Inspector.
 -Fixes for using PreviewField with general Assets (like Images / sounds)

## 1.2.1
 -Added example assets and better documentation.

## 1.2.0
 -First release