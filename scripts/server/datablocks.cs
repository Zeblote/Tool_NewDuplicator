// * ######################################################################
// *
// *    New Duplicator - Scripts - Server
// *    Datablocks
// *
// *    -------------------------------------------------------------------
// *    Creates datablocks required for the new duplicator
// *
// * ######################################################################

//Basic golden duplicator
///////////////////////////////////////////////////////////////////////////

//Duplicator Item
datablock ItemData(ND_Item)
{
	cameraMaxDist   = 0.1;
	canDrop         = 1;
	category        = "Tools";
	className       = "Weapon";
	density         = 0.2;
	doColorShift    = false;
	colorShiftColor = "1 0.84 0 1";
	elasticity      = 0.2;
	emap            = 1;
	friction        = 0.6;
	iconName        = $ND::ResourcePath @ "server/icon";
	image           = "ND_Image";
	shapeFile       = $ND::ResourcePath @ "server/duplicator_brick.dts";
	uiName          = "Duplicator";
};

//Particles for explosion
datablock ParticleData(ND_HitParticle)
{
	colors[0]          = "1 0.84 0 0.9";
	colors[1]          = "1 0.84 0 0.7";
	colors[2]          = "1 0.84 0 0.5";
	gravityCoefficient = 0.7;
	lifetimeMS         = 600;
	lifetimeVarianceMS = 200;
	sizes[0]           = 0.6;
	sizes[1]           = 0.4;
	sizes[2]           = 0.3;
	spinRandomMax      = 90;
	spinRandomMin      = -90;
	textureName        = "base/client/ui/brickIcons/2x2";
	times[1]           = 0.8;
	times[2]           = 1;
};

//Emitter for explosion
datablock ParticleEmitterData(ND_HitEmitter)
{
	lifetimeMS       = 20;
	ejectionPeriodMS = 1;
	periodVarianceMS = 0;
	ejectionVelocity = 3;
	ejectionOffset   = 0.2;
	particles        = ND_HitParticle;
	thetaMin         = 20;
	thetaMax         = 80;
	velocityVariance = 0;
};

//Explosion
datablock ExplosionData(ND_HitExplosion)
{
	camShakeDuration = 0.5;
	camShakeFreq     = "1 1 1";
	emitter[0]       = ND_HitEmitter;
	faceViewer       = 1;
	lifetimeMS       = 180;
	lightEndRadius   = 0;
	lightStartColor  = "0 0 0 0";
	lightStartRadius = 0;
	shakeCamera      = 1;
	soundProfile     = "wandHitSound";
};

//Projectile to make explosion
datablock ProjectileData(ND_HitProjectile)
{
	bounceElasticity = 0;
	bounceFriction   = 0;
	explodeOnDeath   = 1;
	explosion        = ND_HitExplosion;
	fadeDelay        = 2;
	gravityMod       = 0;
	lifetime         = 0;
	range            = 10;
};

//Swing particles
datablock ParticleData(ND_WaitParticle)
{
	colors[0]          = "1 0.84 0 0.9";
	colors[1]          = "1 0.84 0 0.7";
	colors[2]          = "1 0.84 0 0.5";
	gravityCoefficient = -0.4;
	dragCoefficient    = 2;
	lifetimeMS         = 400;
	lifetimeVarianceMS = 200;
	sizes[0]           = 0.5;
	sizes[1]           = 0.8;
	sizes[2]           = 0;
	spinRandomMax      = 0;
	spinRandomMin      = 0;
	textureName        = "base/client/ui/brickIcons/1x1";
	times[1]           = 0.5;
	times[2]           = 1;
};

//Swing emitter
datablock ParticleEmitterData(ND_WaitEmitter)
{
	lifetimeMS       = 5000;
	ejectionPeriodMS = 10;
	periodVarianceMS = 0;
	ejectionVelocity = 1;
	ejectionOffset   = 0.01;
	particles        = ND_WaitParticle;
	thetaMin         = 20;
	thetaMax         = 80;
	velocityVariance = 0;
};

//Spin particles
datablock ParticleData(ND_SpinParticle : ND_WaitParticle)
{
	colors[0]          = "1 0.65 0 0.9";
	colors[1]          = "1 0.65 0 0.7";
	colors[2]          = "1 0.65 0 0.5";
	gravityCoefficient = 0;
	sizes[0]           = 0.3;
	sizes[1]           = 0.5;
	sizes[2]           = 0;
	textureName        = "base/client/ui/brickIcons/1x1";
};

//Spin emitter
datablock ParticleEmitterData(ND_SpinEmitter : ND_WaitEmitter)
{
	particles        = ND_SpinParticle;
	ejectionPeriodMS = 15;
	thetaMin         = 40;
	thetaMax         = 140;
	ejectionVelocity = 2;
};

//Duplicator image
datablock ShapeBaseImageData(ND_Image)
{
	shapeFile       = $ND::ResourcePath @ "server/duplicator_brick.dts";
	className       = "WeaponImage";
	emap            = true;
	mountPoint      = 0;
	offset          = "0 0 0";
	eyeOffset       = "0.7 1.4 -0.9";
	armReady        = true;
	showBricks      = true;
	doColorShift    = true;
	colorShiftColor = "1 0.84 0 1";
	item            = ND_Item;
	projectile      = ND_HitProjectile;
	loaded            = false;

	//Image states
	stateName[0]                    = "Activate";
	stateSpinThread[0]              = "Stop";
	stateTimeoutValue[0]            = 0;
	stateAllowImageChange[0]        = false;
	stateTransitionOnTimeout[0]     = "Idle";

	stateName[1]                    = "Idle";
	stateSpinThread[1]              = "Stop";
	stateAllowImageChange[1]        = true;
	stateTransitionOnNotLoaded[1]   = "StartSpinning";
	stateTransitionOnTriggerDown[1] = "PreFire";

	stateName[2]                    = "PreFire";
	stateScript[2]                  = "onPreFire";
	stateTimeoutValue[2]            = 0.01;
	stateAllowImageChange[2]        = false;
	stateTransitionOnTimeout[2]     = "Fire";

	stateName[3]                    = "Fire";
	stateFire[3]                    = true;
	stateScript[3]                  = "onFire";
	stateEmitter[3]                 = ND_WaitEmitter;
	stateSequence[3]                = "swing";
	stateEmitterNode[3]             = "muzzlePoint";
	stateEmitterTime[3]             = 0.4;
	stateTimeoutValue[3]            = 0.4;
	stateWaitForTimeout[3]          = true;
	stateAllowImageChange[3]        = false;
	stateTransitionOnTimeout[3]     = "CheckFire";

	stateName[4]                    = "CheckFire";
	stateSpinThread[4]              = "Stop";
	stateTransitionOnNotLoaded[4]   = "StartSpinning_TDown";
	stateTransitionOnTriggerUp[4]   = "Idle";

	//Spinning states (from idle)
	stateName[5]                    = "StartSpinning";
	stateSpinThread[5]              = "SpinUp";
	stateTimeoutValue[5]            = 0.25;
	stateTransitionOnTimeout[5]     = "IdleSpinning";

	stateName[6]                    = "IdleSpinning";
	stateEmitter[6]                 = ND_SpinEmitter;
	stateSpinThread[6]              = "FullSpeed";
	stateEmitterNode[6] 		    = "muzzlePoint";
	stateEmitterTime[6] 		    = 0.35;
	stateTimeoutValue[6]            = 0.35;
	stateTransitionOnLoaded[6]      = "StopSpinning";
	stateTransitionOnTimeout[6]     = "IdleSpinning";

	stateName[7]                    = "StopSpinning";
	stateSpinThread[7]              = "SpinDown";
	stateTimeoutValue[7]            = 0.25;
	stateTransitionOnTimeout[7]     = "Idle";

	//Spinning states (from checkfire, trigger is still down)
	stateName[8]                    = "StartSpinning_TDown";
	stateSpinThread[8]              = "SpinUp";
	stateTimeoutValue[8]            = 0.25;
	stateTransitionOnTimeout[8]     = "IdleSpinning_TDown";

	stateName[9]                    = "IdleSpinning_TDown";
	stateEmitter[9]                 = ND_SpinEmitter;
	stateSpinThread[9]              = "FullSpeed";
	stateEmitterNode[9]             = "muzzlePoint_TDown";
	stateEmitterTime[9]             = 0.4;
	stateTimeoutValue[9]            = 0.4;
	stateTransitionOnLoaded[9]      = "StopSpinning_TDown";
	stateTransitionOnTimeout[9]     = "IdleSpinning_TDown";

	stateName[10]                   = "StopSpinning_TDown";
	stateSpinThread[10]             = "SpinDown";
	stateTimeoutValue[10]           = 0.25;
	stateTransitionOnTimeout[10]    = "CheckFire";
};


//Spinning selection box for box mode
///////////////////////////////////////////////////////////////////////////

//Duplicator image
datablock ShapeBaseImageData(ND_Image_Box : ND_Image)
{
	shapeFile = $ND::ResourcePath @ "server/duplicator_selection.dts";
};


//Blue duplicator for plant mode
///////////////////////////////////////////////////////////////////////////

//Particles for explosion
datablock ParticleData(ND_HitParticle_Blue : ND_HitParticle)
{
	colors[0] = "0 0.25 1 0.9";
	colors[1] = "0 0.25 1 0.7";
	colors[2] = "0 0.25 1 0.5";
};

//Emitter for explosion
datablock ParticleEmitterData(ND_HitEmitter_Blue : ND_HitEmitter)
{
	particles = ND_HitParticle_Blue;
};

//Explosion
datablock ExplosionData(ND_HitExplosion_Blue : ND_HitExplosion)
{
	emitter[0] = ND_HitEmitter_Blue;
};

//Projectile to make explosion
datablock ProjectileData(ND_HitProjectile_Blue : ND_HitProjectile)
{
	explosion = ND_HitExplosion_Blue;
};

//Swing particles
datablock ParticleData(ND_WaitParticle_Blue : ND_WaitParticle)
{
	colors[0] = "0 0.25 1 0.9";
	colors[1] = "0 0.25 1 0.7";
	colors[2] = "0 0.25 1 0.5";
};

//Swing emitter
datablock ParticleEmitterData(ND_WaitEmitter_Blue : ND_WaitEmitter)
{
	particles = ND_WaitParticle_Blue;
};

//Spin particles
datablock ParticleData(ND_SpinParticle_Blue : ND_SpinParticle)
{
	colors[0] = "0 0.25 0.75 0.9";
	colors[1] = "0 0.25 0.75 0.7";
	colors[2] = "0 0.25 0.75 0.5";
};

//Spin emitter
datablock ParticleEmitterData(ND_SpinEmitter_Blue : ND_SpinEmitter)
{
	particles = ND_SpinParticle_Blue;
};

//Duplicator image
datablock ShapeBaseImageData(ND_Image_Blue : ND_Image)
{
	colorShiftColor = "0 0.25 1 1";
	projectile      = ND_HitProjectile_Blue;

	//Image states
	stateEmitter[3]  = ND_WaitEmitter_Blue;
	stateEmitter[6]  = ND_SpinEmitter_Blue;
	stateEmitter[9] = ND_SpinEmitter_Blue;
};


//Resizable selection and highlight box
///////////////////////////////////////////////////////////////////////////

//Transparent box to visualize bricks intersecting selection box
datablock StaticShapeData(ND_SelectionBoxOuter)
{
	shapeFile = $ND::ResourcePath @ "server/selectionbox_outer.dts";
};

//Inside box (inverted normals) to visualize backfaces
datablock StaticShapeData(ND_SelectionBoxInner)
{
	shapeFile = $ND::ResourcePath @ "server/selectionbox_inner.dts";
};

//Small box to create solid edges
datablock StaticShapeData(ND_SelectionBoxBorder)
{
	shapeFile = $ND::ResourcePath @ "server/selectionbox_border.dts";
};

//Empty shape to hold shapename
datablock StaticShapeData(ND_SelectionBoxShapeName)
{
	shapeFile = "base/data/shapes/empty.dts";
};
