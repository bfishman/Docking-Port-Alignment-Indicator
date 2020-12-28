# Docking Port Alignment Indicator

You can contribute towards the Docking Port Alignment Indicator mod by
submitting bug fixes or new features.

## Getting Started

1. Clone the [git repository][DPAI_Git].
2. Copy "Source/DockingPortAlignmentIndicator/user_settings.props.template" to
"Source/DockingPortAlignmentIndicator/user_settings.props" and edit the
"KSPDIR" variable to point to your KSP directory.
3. Open the Solution file and compile the project.
4. Run "scripts/deploy.bat" or "scripts/deploy-debug.bat" to compile and copy
DPAI into your KSP directory.

Please see [this forum thread][KSP_Debugging] for setting up a debugging
environemnt, this can be very useful to track down issues.

## Making Changes

Create your own git branch to work on a feature or a fix. Remember to commit
early and often and give each commit a detailed description.

Add your changes to "CHANGELOG.md"

Update the version in "AssemblyInfo.cs" appropriately.

Update "scripts/Version.template" if the KSP version number has changed.

## Submitting Changes

Update your git repo and merge in the latest "master" branch into your
development branch.

Push your development branch to your copy of DPAI on GitHub and create a
pull request.

## Making a Release

Run "scripts/release.bat". This will compile a release build, generate version
and changelog files, and create a DPAI ZIP file for releasing.


[DPAI_Git]: https://github.com/bfishman/Docking-Port-Alignment-Indicator
[KSP_Debugging]: https://forum.kerbalspaceprogram.com/index.php?showtopic=102909
