#!/usr/bin/python3
# -*- coding: utf-8 -*-

import sys

arguments_list = sys.argv
if len(arguments_list) != 5:
    print ("Need arguments looks like: file name, TOF, StartTime, EndTime")
    print ("Example: MIKEHYDRO_River.mhydro, '2002 05 27 23:30:00', '2002 05 23 00:00:00', '2002 05 27 23:30:00'")
    input("Press Enter to exit...")
    sys.exit(0)

#arguments
input_file = arguments_list[1]
tof = arguments_list[2]
start_time = arguments_list[3]
end_time = arguments_list[4]

looking_for = ("TOF ", "StartTime ", "EndTime ")
split_key = "= "

output_file = []

try:
    file = open(input_file, "r")
except FileExistsError as err:
    print(str(err))
else:
    file_list = file.readlines()
    file.close()
    for line in file_list:
        new_data = ""
        if looking_for[0] in line:
            new_data = tof
        elif looking_for[1] in line:
            new_data = start_time
        elif looking_for[2] in line:
            new_data = end_time

        if new_data != "":
            line = line.split(split_key)
            line = line[0] + split_key + new_data + "\n"

        output_file.append(line)

    file = open(input_file, "w")
    file.writelines(output_file)
    file.close()
