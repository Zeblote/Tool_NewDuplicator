// * ######################################################################
// *
// *    New Duplicator - Server
// *    Main Script
// *
// *    -------------------------------------------------------------------
// *    Executes required classes and scripts, initializes server side
// *
// * ######################################################################

$ND::Version = "1.2.0";

$ND::FilePath = filePath($Con::File) @ "/";
$ND::ConfigPath = "config/NewDuplicator/";

$ND::ClassPath = $ND::FilePath @ "classes/";
$ND::ScriptPath = $ND::FilePath @ "scripts/";
$ND::ResourcePath = $ND::FilePath @ "resources/";

if(isObject(ND_ServerGroup))
	ND_ServerGroup.delete();

new ScriptGroup(ND_ServerGroup);

echo(" \n--- Loading Server Classes ---");
exec($ND::ClassPath @ "server/ghostgroup.cs");
exec($ND::ClassPath @ "server/highlightbox.cs");
exec($ND::ClassPath @ "server/selection.cs");
exec($ND::ClassPath @ "server/selectionbox.cs");
exec($ND::ClassPath @ "server/undogrouppaint.cs");
exec($ND::ClassPath @ "server/undogroupplant.cs");
exec($ND::ClassPath @ "server/undogroupwrench.cs");

exec($ND::ClassPath @ "server/duplimode/cubeselect.cs");
exec($ND::ClassPath @ "server/duplimode/cubeselectprogress.cs");
exec($ND::ClassPath @ "server/duplimode/cutprogress.cs");
exec($ND::ClassPath @ "server/duplimode/fillcolor.cs");
exec($ND::ClassPath @ "server/duplimode/fillcolorprogress.cs");
exec($ND::ClassPath @ "server/duplimode/loadprogress.cs");
exec($ND::ClassPath @ "server/duplimode/plantcopy.cs");
exec($ND::ClassPath @ "server/duplimode/plantcopyprogress.cs");
exec($ND::ClassPath @ "server/duplimode/saveprogress.cs");
exec($ND::ClassPath @ "server/duplimode/stackselect.cs");
exec($ND::ClassPath @ "server/duplimode/stackselectprogress.cs");
exec($ND::ClassPath @ "server/duplimode/wrenchprogress.cs");

echo(" \n--- Loading Server Scripts ---");
exec($ND::ScriptPath @ "server/commands.cs");
exec($ND::ScriptPath @ "server/datablocks.cs");
exec($ND::ScriptPath @ "server/functions.cs");
exec($ND::ScriptPath @ "server/handshake.cs");
exec($ND::ScriptPath @ "server/highlight.cs");
exec($ND::ScriptPath @ "server/images.cs");
exec($ND::ScriptPath @ "server/modes.cs");
exec($ND::ScriptPath @ "server/prefs.cs");
exec($ND::ScriptPath @ "server/symmetrydefinitions.cs");
exec($ND::ScriptPath @ "server/symmetrytable.cs");
exec($ND::ScriptPath @ "server/undo.cs");

echo(" \n--- Initializing Server ---");
activatePackage(NewDuplicator_Server);
schedule(10, 0, activatePackage, NewDuplicator_Server_Final);

$ND::BrickHighlightColor = ndGetClosestColorID("255 255 0");
ndRegisterDuplicatorModes();
ndAutoRegisterPrefs();
