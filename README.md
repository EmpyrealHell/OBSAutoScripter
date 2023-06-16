# OBSAutoScripter
Create and play scripted sequences in OBS

# Usage
Run the application. Optionally provide the filename of a script to execute. If no script is provided, script.json will be used.
```
OBSAutoScripter.exe script.json
```

## Credentials
You must provide your OBS url, port, and password in the credentials.json file. This file must be placed in the same folder as the executable. See the Credentials.json file in the Data folder for an example of the expected format.

# File Format
See the example.json file in the Data folder for an example of the expected format.

There are three main sections of the script file.
## Scenes
The scenes collection is where you list out all of scenes and scene items (sources) you want to be able to interact with. Key is used to refence this element in sequence steps. It is recommended you start all keys with '$', but it is not required.

## Sequences
The sequences collection is where you detail the series of steps for each sequence. Name is used to refence the sequence in conditions.
The steps collection in each sequence details the steps to be played when this sequence is executed. Steps are executed sequentially and immediately. There is no timing information retrieved from OBS, so if you need to wait for animations to play out, use the Wait step action.
The valid step actions are as follows:

| Action | Description | Example |
| ------ | ----------- | ------- |
| None | Does nothing. Can be used to disable individual steps. | { "Action": "None" } |
| Wait | Delays execution. Duration is in milliseconds. | { "Action": "Wait", "Duration": 1000 } |
| EnableSource | Show a source. Target references a Key from the Scenes collection. | { "Action": "EnableSource", "Target": "$SceneItem" } |
| DisableSource | Hide a source. Target references a Key from the Scenes collection. | { "Action": "DisableSource", "Target": "$SceneItem" } |
| ChangeScene | Changes the active scene. Target references a Key from the Scenes collection. | { "Action": "ChangeScene", "Target": "$Scene" } |
| CopyPosition | Copies the position of one source to another. Sources do not have to be in the same scene. Source and Target reference Keys from the Scenes collection. An optional offset can be supplied. | { "Action": "CopyPosition", "Source": "$SceneItem1", "Target": "$SceneItem2", "OffsetX": 100, "OffsetY": 100 } |
| CopySize | Copies the size of one source to another. Sources do not have to be in the same scene. Source and Target reference Keys from the Scenes collection. | { "Action": "CopySize", "Source": "$SceneItem1", "Target": "$SceneItem2" } |
| CopyRotation | Copies the rotation of one source to another. Sources do not have to be in the same scene. Source and Target reference Keys from the Scenes collection. | { "Action": "CopyRotation", "Source": "$SceneItem1", "Target": "$SceneItem2" } |
| CopyCrop | Copies the crop of one source to another. Sources do not have to be in the same scene. Source and Target reference Keys from the Scenes collection. | { "Action": "CopyCrop", "Source": "$SceneItem1", "Target": "$SceneItem2" } |
| SetPosition | Sets the position of a source. Target references a Key from the Scenes collection. | { "Action": "SetPosition", "Target": "$SceneItem", "X": 100, "Y": 100 } |
| SetSize | Sets the size of a source. Target references a Key from the Scenes collection. Size is in pixels, but applied by modifying the scale value. | { "Action": "SetSize", "Target": "$SceneItem", "Width": 100, "Height": 100 } |
| SetRotation | Sets the rotation of a source. Target references a Key from the Scenes collection. Angle is in clockwise degrees. | { "Action": "SetRotation", "Target": "$SceneItem", "Angle": 180 } |
| SetCrop | Sets the crop of a source. Target references a Key from the Scenes collection. | { "Action": "SetCrop", "Target": "$SceneItem", "X": 10, "Y": 10, "Width": 100, "Height": 100 } |

## Conditions
The conditions collection allows you to specify conditions that must be met for a specific sequence to play. If you do not provide any conditions, then the first sequence will be played. If conditions are provided but none are met, no sequence is played.
The valid conditions are as follows:

| Condition | Description | Example |
| --------- | ----------- | ------- |
| Always | A condition that is always met. This can be used as a "default" action if placed at the bottom of the collection. | { "Function": "Always", "Sequence": "SequenceName" } |
| Never | A condition that is never met. This can be used to disable a sequence. | { "Function": "Never", "Sequence": "SequenceName" } |
| Never | A condition that is never met. This can be used to disable a sequence. | { "Function": "Never", "Sequence": "SequenceName" } |
| And | Executes if all sub conditions are met. | { "Function": "And", "SubConditions": [ { "Function": "Always" } ] } |
| Or | Executes if any sub condition is met. | { "Function": "Or", "SubConditions": [ { "Function": "Always" } ] } |
| Not | Executes if no sub conditions are met. | { "Function": "Not", "SubConditions": [ { "Function": "Always" } ] } |
| CurrentScene | Executes if the current scene matches the specified value. Value references a Key from the Scenes collection. | { "Function": "CurrentScene", "Value": "$Scene" } |
| SourceEnabled | Executes if the specified source is currently enabled. Value references a Key from the Scenes collection. | { "Function": "SourceEnabled", "Value": "$SceneItem" } |
| SourceDisabled | Executes if the specified source is currently disabled. Value references a Key from the Scenes collection. | { "Function": "SourceDisabled", "Value": "$SceneItem" } |
