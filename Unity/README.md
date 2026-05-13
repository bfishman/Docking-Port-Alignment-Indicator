# Docking Port Alignment Indicator - Unity Projects

This folder contains the Unity projects used to build the Docking Port
Alignment Indicator (DPAI). To work on the Unity projects, open this folder
using Unity 2019.4.18f1.

The DPAI assets can be found in the "DPAI" folder, which also contains the
scenes to edit.

The following scenes exist in the project:

- DPAI_Interface - the main GUI for DPAI
- DPAI_KSPedia - the KSPedia pages for DPAI

## Resources

- [Unity Interface Tutorial][1] thread on the KSP Forum.
- [KSPedia Tutorial][2] thread on the KSP Forum.

## Editing

Double-click on the scene to edit; it will appear in the "Hierarchy". From
there it can be edited.

### DPAI_Interface

This consists of a simple Canvas and Panel on which the KSP background is
embedded with the Prev/Next buttons and a text box to display the currently
targetted docking port.

This is saved as a prefab and generated into the "dpai" asset bundle.

### KSPedia

This consists of a number of screens each of which starts with a Canvas and a
Panel. To edit a specific one, hide the others. 

The screens are arranged using the "KSPAssets -> KSPedia" editor.

Ensure all screens are toggled to be visible prior to generating the
"kspedia_dpai" asset bundle.

[1]: https://forum.kerbalspaceprogram.com/topic/151354-unity-ui-creation-tutorial/
[2]: https://forum.kerbalspaceprogram.com/topic/137628-kspedia-creation-tutorial/
