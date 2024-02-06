#!/usr/bin/env bash

clear
./build.sh
if (( $? == 0 )); then
	otd
fi