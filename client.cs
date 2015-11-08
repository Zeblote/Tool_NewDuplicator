// * ######################################################################
// *
// *    New Duplicator - Client
// *    Main Script
// *
// *    -------------------------------------------------------------------
// *    Initialize the New Duplicator
// *
// * ######################################################################

//Version
$ND::Version = "0.1.0";

//Step 1: Path Config
$ND::FilePath = filePath($Con::File) @ "/";
$ND::ConfigPath = "config/NewDuplicator/";

$ND::ClassPath = $ND::FilePath @ "classes/";
$ND::ScriptPath = $ND::FilePath @ "scripts/";
$ND::ResourcePath = $ND::FilePath @ "resources/";

//Step 2: Create Group
new ScriptGroup(ND_ClientGroup);

//Step 3: Execute Scripts
echo(" \n--- Loading Client Scripts ---");
exec($ND::ScriptPath @ "client/rotatebrick.cs");
exec($ND::ScriptPath @ "client/handshake.cs");

//Step 4: Load Classes
//echo(" \n--- Loading Client Classes ---");

//Step 5: Start New Duplicator
echo(" \n--- Starting New Duplicator Client ---");
activatePackage(NewDuplicator_Client);
