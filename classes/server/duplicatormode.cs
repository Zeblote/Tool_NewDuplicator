// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    NewDuplicatorMode
// *
// *    -------------------------------------------------------------------
// *    Duplicator modes receive callbacks for actions done by the client
// *
// * ######################################################################

//Possible dupli modes
$NDM::Disabled            = 0;
$NDM::CubeSelect          = 1;
$NDM::CubeSelectProgress  = 2;
$NDM::StackSelect         = 3;
$NDM::StackSelectProgress = 4;
$NDM::PlantCopy           = 5;
$NDM::PlantCopyProgress   = 6;

//Disabled duplicator mode (does nothing)
ND_ServerGroup.add(
	new ScriptObject(NDM_Disabled)
	{
		class = "NewDuplicatorMode";
		index = $NDM::Disabled;

		allowSelecting = false;
		allowUnMount   = false;
	}
);



//Changing modes
///////////////////////////////////////////////////////////////////////////

function NewDuplicatorMode::onStartMode(%this, %client, %lastMode){}
function NewDuplicatorMode::onChangeMode(%this, %client, %nextMode){}
function NewDuplicatorMode::onKillMode(%this, %client){}

//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

function NewDuplicatorMode::onSelectObject(%this, %client, %obj, %pos, %normal){}

//Generic inputs
///////////////////////////////////////////////////////////////////////////

function NewDuplicatorMode::onLight(%this, %client){}
function NewDuplicatorMode::onNextSeat(%this, %client){}
function NewDuplicatorMode::onPrevSeat(%this, %client){}
function NewDuplicatorMode::onShiftBrick(%this, %client, %x, %y, %z){}
function NewDuplicatorMode::onSuperShiftBrick(%this, %client, %x, %y, %z){}
function NewDuplicatorMode::onRotateBrick(%this, %client, %direction){}
function NewDuplicatorMode::onPlantBrick(%this, %client){}
function NewDuplicatorMode::onCancelBrick(%this, %client){}

//Interface
///////////////////////////////////////////////////////////////////////////

function NewDuplicatorMode::getBottomPrint(%this, %client){}
