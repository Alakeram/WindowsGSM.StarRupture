# WindowsGSM.StarRupture

WindowsGSM plugin that creates a StarRupture Dedicated server!



* Created by Alakeram



# Requirements

WindowsGSM >= 1.21.0



# The Game

* Game
  https://store.steampowered.com/app/1631270/StarRupture/



# Installation

1. Download the latest release
2. Move **WindowsGSM.StarRupture** Zip file into your WindowsGSM Folder.
3. Click on "Plugin"
4. Click on "Import Plugin"
5. Select the ZIP in the folder
6. Wait for the plugin to load.
7. If the plugin doesn't show up, **\[RELOAD PLUGINS]** button or restart WindowsGSM



# Updating Plugin



Before updating plugin, PLEASE backup your save files just in case some of the settings that are created do not break your save.

If you have a server already made on plugin before 1.6 you will need to open the DSSettings.txt file and change the session name to what you named it originally (Can be found in your save file location as the filename)



Also make sure to just replace the StarRupture.cs file in the plugin's folder (GSM Servers\\plugins\\StarRupture.cs)



# Starting A Server

**\*This may change depending on updates\***



If you want a Password on the server, you will need to go to https://starrupture.agngaming.com/passwords/ and generate a password, I don't have the RSA PK to allow the windows GSM to create one.



paste it into the password json files at root level:



serverfiles\\PlayerPassword.json (Server password)

serverfiles\\password.json (Server Settings Password)





# Joining The Server

**\*Attempting to join the server using local IP Might BREAK the server, Do not attempt unless you want the possibility of losing your save!\***



1. Get your servers PUBLIC IP (Currently you cannot join dedicated servers with your local IP/Port)
2. Click on Join Game (On Main Menu)
3. Click Dedicated Server
4. Enter your PUBLIC IP and Port (xxx.xxx.xxx.xxx:7777), and server password (This was set on the step 5 of Starting A Server)



# License

This project is licensed under the MIT License  - see the [LICENSE.md](LICENSE) file for details

