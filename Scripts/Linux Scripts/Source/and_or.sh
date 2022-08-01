#! /bin/bash

read -p "yoshingizni kiriting :" age

#if [ $age -gt 18 ] && [ $age -lt 45 ]
#if [ $age -gt 18 -a $age -lt 45 ]
#if [[ $age -gt 18 && $age -lt 45 ]]  # va operatorining ko'rinishi
#if [ $age -eq 18 ] || [ $age -eq 20 ]
#if [ $age -eq 18 -o $age -eq 20 ]
if [[ $age -eq 18 || $age -eq 20 ]]
then  
    echo siz ushbu saytga kirishingiz mumkin.
else 
    echo siga ushbu saytga kirishga ruhsat yoq
fi