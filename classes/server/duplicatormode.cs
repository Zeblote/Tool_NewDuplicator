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
$NDDM::Disabled            = 0;
$NDDM::CubeSelect          = 1;
$NDDM::CubeSelectProgress  = 2;
$NDDM::StackSelect         = 3;
$NDDM::StackSelectProgress = 4;
$NDDM::PlantCopy           = 5;
$NDDM::PlantCopyProgress   = 6;

//Disabled duplicator mode (does nothing)
ND_ServerGroup.add(
	new ScriptObject(NDDM_Disabled)
	{
		class = "NewDuplicatorMode";
		index = $NDDM::Disabled;

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
