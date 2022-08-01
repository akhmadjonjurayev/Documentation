#! /bin/bash

#FILE=/etc/docker
#if [ ! -f "$FILE" ]; then
#   echo "$FILE does not exist."
#fi

read -p "faylni nomini kiriting :" file
if [ -f $file ];
then 
    if [ -w $file ];
    then 
        cat >> $file
    else 
        echo permission dinaied
    fi
else 
    echo fayl mavjud emas
fi        
