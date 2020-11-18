#!/usr/bin/python3
# -*- coding: utf-8 -*-

import sys

arguments_list = sys.argv
if len(arguments_list) != 3:
    print ("Need arguments looks like: mhydro file name, RuntimeInfo")
    print ("Example: MIKEHYDRO_River.mhydro, RuntimeInfo.xml)
    input("Press Enter to exit...")
    sys.exit(0)

#arguments
input_file = arguments_list[1]
info_file = arguments_list[2]

looking_for = ("startDateTime ", "endDateTime ", "time0 ")
split_key = '"'

try:
    file = open(info_file, "r")
except FileExistsError as err:
    print(str(err))
    input("Press Enter to exit...")
    sys.exit(0)
else:
    file_list = file.readlines()
    file.close()

    for line in file_list:
        split_line = line.split(split_key)
        if looking_for[0] in line:
            start_time = split_line[1] + " " + split_line[3]
        elif looking_for[1] in line:
            end_time = split_line[1] + " " + split_line[3]
        elif looking_for[2] in line:
            tof = split_line[1] + " " + split_line[3]

looking_for = ("TOF ", "StartTime ", "EndTime ")
split_key = "= "

output_file = []

try:
    file = open(input_file, "r")
except FileExistsError as err:
    print(str(err))
    input("Press Enter to exit...")
    sys.exit(0)
else:
    file_list = file.readlines()
    file.close()
    for line in file_list:
        new_data = ""
        if looking_for[0] in line:
            new_data = "'" + tof + "'"
        elif looking_for[1] in line:
            new_data = "'" + start_time + "'"
        elif looking_for[2] in line:
            new_data = "'" + end_time + "'"

        if new_data != "":
            line = line.split(split_key)
            line = line[0] + split_key + new_data + "\n"

        output_file.append(line)

    file = open(input_file, "w")
    file.writelines(output_file)
    file.close()
