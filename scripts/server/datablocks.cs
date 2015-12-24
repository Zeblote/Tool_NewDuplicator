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
	category        = "Weapon";
	className       = "Tool";
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
	lifetimeMS       = 400;
	ejectionPeriodMS = 10;
	periodVarianceMS = 0;
	ejectionVelocity = 1;
	ejectionOffset   = 0.01;
	particles        = ND_WaitParticle;
	thetaMin         = 20;
	thetaMax         = 80;
	velocityVariance = 0;
};

//Duplicator image
datablock ShapeBaseImageData(ND_Image)
{
	shapeFile       = $ND::ResourcePath @ "server/duplicator_brick.dts";
	className       = "WeaponImage";
	emap            = true;
	mountPoint      = 0;
	offset          = "0 0 0";
	eyeOffset       = "0.6 1.2 -0.6";
	armReady        = true;
	showBricks      = true;
	doColorShift    = true;
	colorShiftColor = "1 0.84 0 1";
	item            = ND_Item;
	projectile      = ND_HitProjectile;

	//Image states
	stateName[0]                    = "Activate";
	stateTimeoutValue[0]            = 0;
	stateAllowImageChange[0]        = false;
	stateTransitionOnTimeout[0]     = "Idle";

	stateName[1]                    = "Idle";
	stateAllowImageChange[1]        = true;
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
	stateEmitterNode[3] 		    = "muzzlePoint";
	stateEmitterTime[3] 		    = 0.4;
	stateTimeoutValue[3]            = 0.4;
	stateWaitForTimeout[3]          = true;
	stateAllowImageChange[3]        = false;
	stateTransitionOnTimeout[3]     = "CheckFire";

	stateName[4]                    = "CheckFire";
	stateTransitionOnTriggerUp[4]   = "PostFire";

	stateName[5]                    = "PostFire";
	stateScript[5]                  = "onPostFire";
	stateTimeoutValue[5]            = "0.01";
	stateWaitForTimeout[5]          = true;
	stateAllowImageChange[5]        = false;
	stateTransitionOnTimeout[5]     = "Idle";
};


//Spinning selection cube for cubic mode
///////////////////////////////////////////////////////////////////////////


//Duplicator image
datablock ShapeBaseImageData(ND_Image_Cube : ND_Image)
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

//Duplicator image
datablock ShapeBaseImageData(ND_Image_Blue : ND_Image)
{
	colorShiftColor = "0 0.25 1 1";
	projectile      = ND_HitProjectile_Blue;

	//Image states
	stateEmitter[3] = ND_WaitEmitter_Blue;
};


//Resizable selection and highlight box
///////////////////////////////////////////////////////////////////////////

//Transparent cube to visualize bricks intersecting selection box
datablock StaticShapeData(ND_SelectionCubeOuter)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_outer.dts";
};

//Inside cube (inverted normals) to visualize backfaces
datablock StaticShapeData(ND_SelectionCubeInner)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_inner.dts";
};

//Small cube to create solid edges
datablock StaticShapeData(ND_SelectionCubeBorder)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_border.dts";
};

//Empty shape to hold shapename
datablock StaticShapeData(ND_SelectionCubeShapeName)
{
	shapeFile = "base/data/shapes/empty.dts";
};
