#!/bin/bash
# Builds and packages the release

if ! command -v msbuild &>/dev/null
then
	echo Could not find 'msbuild' in the path. Please ensure it is installed properly.
	exit 1
fi

cd "$(dirname "$0")"
msbuild Deploy.proj /target:Release

