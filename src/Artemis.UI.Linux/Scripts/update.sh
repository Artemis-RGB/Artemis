#!/bin/bash

# This script is used to update Artemis on Linux.
# Arguments:
#    $1 = sourceDirectory, the directory where the new files are located
#    $2 = destinationDirectory, the directory where the old files are located
#    $3 = artemisArgs, the arguments to pass to the Artemis executable

sourceDirectory=$1
destinationDirectory=$2
artemisArgs=$3

echo "sourceDirectory $sourceDirectory"
echo "destinationDirectory $destinationDirectory"
echo "artemisArgs $artemisArgs"
exit 0

# Wait for up to 10 seconds for the Artemis process to exit
i=0
while [ $i -le 10 ]
do
    if ! pgrep -x "Artemis.UI.Linux" > /dev/null
    then
        break
    fi
    sleep 1
    i=$((i+1))
done

# If the Artemis process is still running, kill it
if pgrep -x "Artemis.UI.Linux" > /dev/null
then
    pkill -x "Artemis.UI.Linux"
fi

# Check if the destination directory exists
if [ ! -d "$destinationDirectory" ]
then
    echo "Destination directory does not exist"
    exit 1
fi

# Clear the destination directory but don't remove it
echo "Cleaning up old version where needed"
rm -rf "{$destinationDirectory:?}/"*

# Move the contents of the source directory to the destination directory
echo "Installing new files"
mv "$sourceDirectory"/* "$destinationDirectory"

# Remove the now empty source directory
rmdir "$sourceDirectory"

echo "Finished! Restarting Artemis"
sleep 1

# When finished, start Artemis again

# If the user has specified arguments, pass them to the executable
if [ -z "$artemisArgs" ]
then
    "$1/Artemis.UI.Linux" &
else
    "$1/Artemis.UI.Linux" "$artemisArgs" &
fi



