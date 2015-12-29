// * ######################################################################
// *
// *    New Duplicator - Server
// *    Main Script
// *
// *    -------------------------------------------------------------------
// *    Executes required classes and scripts, initializes server side
// *
// * ######################################################################

$ND::Version = "1.1.2";

$ND::FilePath = filePath($Con::File) @ "/";
$ND::ConfigPath = "config/NewDuplicator/";

$ND::ClassPath = $ND::FilePath @ "classes/";
$ND::ScriptPath = $ND::FilePath @ "scripts/";
$ND::ResourcePath = $ND::FilePath @ "resources/";

if(isObject(ND_ServerGroup))
	ND_ServerGroup.delete();

new ScriptGroup(ND_ServerGroup);

echo(" \n--- Loading Server Classes ---");
exec($ND::ClassPath @ "server/duplicatormode.cs");
exec($ND::ClassPath @ "server/ghostgroup.cs");
exec($ND::ClassPath @ "server/highlightbox.cs");
exec($ND::ClassPath @ "server/highlightset.cs");
exec($ND::ClassPath @ "server/selection.cs");
exec($ND::ClassPath @ "server/selectionbox.cs");

exec($ND::ClassPath @ "server/duplimode/cubeselect.cs");
exec($ND::ClassPath @ "server/duplimode/cubeselectprogress.cs");
exec($ND::ClassPath @ "server/duplimode/cutprogress.cs");
exec($ND::ClassPath @ "server/duplimode/plantcopy.cs");
exec($ND::ClassPath @ "server/duplimode/plantcopyprogress.cs");
exec($ND::ClassPath @ "server/duplimode/stackselect.cs");
exec($ND::ClassPath @ "server/duplimode/stackselectprogress.cs");

echo(" \n--- Loading Server Scripts ---");
exec($ND::ScriptPath @ "server/datablocks.cs");
exec($ND::ScriptPath @ "server/duplicator.cs");
exec($ND::ScriptPath @ "server/handshake.cs");
exec($ND::ScriptPath @ "server/images.cs");
exec($ND::ScriptPath @ "server/manualsymmetry.cs");
exec($ND::ScriptPath @ "server/prefs.cs");
exec($ND::ScriptPath @ "server/symmetrytable.cs");

echo(" \n--- Initializing Server ---");
activatePackage(NewDuplicator_Server);
schedule(10, 0, activatePackage, NewDuplicator_Server_Final);

$ND::BrickHighlightColor = ndGetClosestColorID("255 255 0");
ndAutoRegisterPrefs();
