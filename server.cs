// * ######################################################################
// *
// *    New Duplicator - Server
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
if(isObject(ND_ServerGroup))
	ND_ServerGroup.delete();

new ScriptGroup(ND_ServerGroup);

//Step 3: Load Classes
echo(" \n--- Loading Server Classes ---");
exec($ND::ClassPath @ "server/prefmanager.cs");

exec($ND::ClassPath @ "server/duplimode.cs");
exec($ND::ClassPath @ "server/duplimode/cubeselect.cs");
exec($ND::ClassPath @ "server/duplimode/cubeselectprogress.cs");
exec($ND::ClassPath @ "server/duplimode/placecopy.cs");
exec($ND::ClassPath @ "server/duplimode/placecopyprogress.cs");
exec($ND::ClassPath @ "server/duplimode/stackselect.cs");
exec($ND::ClassPath @ "server/duplimode/stackselectprogress.cs");

exec($ND::ClassPath @ "server/dupliimage.cs");
exec($ND::ClassPath @ "server/selectionbox.cs");
exec($ND::ClassPath @ "server/selection.cs");
exec($ND::ClassPath @ "server/ghostgroup.cs");
exec($ND::ClassPath @ "server/highlightbox.cs");
exec($ND::ClassPath @ "server/highlightset.cs");

//Step 4: Load Scripts
echo(" \n--- Loading Server Scripts ---");
exec($ND::ScriptPath @ "server/datablocks.cs");
exec($ND::ScriptPath @ "server/duplicator.cs");
exec($ND::ScriptPath @ "server/handshake.cs");

//Step 5: Start New Duplicator
echo(" \n--- Starting New Duplicator Server ---");
activatePackage(NewDuplicator_Server);
ND_PrefManager().registerPrefs();
