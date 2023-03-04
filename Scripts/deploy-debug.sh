#!/bin/bash
# Builds the Debug binaries and deploys them to your KSP Directory

if ! command -v msbuild &>/dev/null
then
	echo Could not find 'msbuild' in the path. Please ensure it is installed properly.
	exit 1
fi

cd "$(dirname "$0")"
msbuild Deploy.proj /target:Deploy-DEBUG

