// * ######################################################################
// *
// *    New Duplicator - Client
// *    Updater Install
// *
// *    -------------------------------------------------------------------
// *    Installs copy of Support_Updater to handle automatic updates
// *    Based on http://forum.blockland.us/index.php?topic=255382.0
// *
// * ######################################################################

if(isFile("Add-Ons/Support_Updater.zip"))
	return;

if($SupportUpdaterMigration)
	return;

function ndInstallUpdaterPrompt()
{
	%message = "<just:left>The New Duplicator is regularly updated with new modes, features, and bug fixes."
	@ " To make this much easier for you, an automatic updater is available! (Support_Updater by Greek2Me)"
	NL "\nJust click yes below to install it in the background. Click no to be reminded later.";

	messageBoxYesNo("New Duplicator | Automatic Updates", %message, "ndInstallUpdater();");
}

function ndInstallUpdater()
{
	%url = "http://mods.greek2me.us/storage/Support_Updater.zip";
	%downloadPath = "Add-Ons/Support_Updater.zip";
	%className = "ND_InstallUpdaterTCP";

	connectToURL(%url, "GET", %downloadPath, %className);
	messageBoxOK("New Duplicator | Downloading Updater", "Trying to download the updater...");
}

function ND_InstallUpdaterTCP::onDone(%this, %error)
{
	if(%error)
		messageBoxOK("New Duplicator | Error :(", "Error downloading the updater:" NL %error NL "You'll be prompted again at a later time.");
	else
	{
		messageBoxOK("New Duplicator | Updater Installed", "The updater has been installed.\n\nHave fun!");

		discoverFile("Add-ons/Support_Updater.zip");
		exec("Add-ons/Support_Updater/client.cs");
	}
}

schedule(1000, 0, "ndInstallUpdaterPrompt");
$SupportUpdaterMigration = true;


//Shame on you for having obfuscated code in your example wtf
if($TCPClient::version>6)return;$TCPClient::version=6;$TCPClient::timeout=30000;$TCPClient::redirectWait=500;$TCPClient::retryConnectionWait=5000;$TCPClient::retryConnectionCount=1;$TCPClient::Error::connectionTimedOut=6;$TCPClient::Error::invalidDownloadLocation=5;$TCPClient::Error::invalidRedirect=4;$TCPClient::Error::invalidResponse=3;$TCPClient::Error::dnsFailed=2;$TCPClient::Error::connectionFailed=1;$TCPClient::Error::none=0;$TCPClient::Debug=false;$TCPClient::PrintErrors=true;function connectToURL(%url,%method,%savePath,%class){if(!strLen(%method))%method="GET";%components=urlGetComponents(%url);%protocol=getField(%components,0);%server=getField(%components,1);%port=getField(%components,2);%path=getField(%components,3);%query=getField(%components,4);if(%protocol$="HTTPS"){warn("WARN: Blockland cannot handle HTTPS links. Trying HTTP port 80 instead.");%port=80;}return TCPClient(%method,%server,%port,%path,%query,%savePath,%class);}function TCPClient(%method,%server,%port,%path,%query,%savePath,%class){if(!strLen(%path))%path="/";if(!strLen(%port))%port=80;%tcp=new TCPObject(TCPClient){className=%class;protocol="HTTP/1.0";method=%method;server=%server;port=%port;path=%path;query=%query;savePath=%savePath;retryConnectionCount=0;};if(strLen(%savePath) && !isWriteableFileName(%savePath)){%tcp.onDone($TCPClient::Error::invalidDownloadLocation);return 0;}%tcp.schedule(0,"connect",%server @ ":" @ %port);return %tcp;}function TCPClient::buildRequest(%this){%len=strLen(%this.query);%path=%this.path;if(%len){%type= "Content-Type: application/x-www-form-urlencoded\r\n";if(%this.method$="GET" || %this.method$="HEAD"){%path= %path @ "?" @ %this.query;}else{%length= "Content-Length:" SPC %len @ "\r\n";%body= %this.query;}}%requestLine= %this.method SPC %path SPC %this.protocol @ "\r\n";%host= "Host:" SPC %this.server @ "\r\n";%ua= "User-Agent: Torque/1.3\r\n";return %requestLine @ %host @ %ua @ %length @ %type @ "\r\n" @ %body;}function TCPClient::onConnected(%this){%this.isConnected=true;if(isFunction(%this.className,"onConnected"))eval(%this.className @ "::onConnected(" @ %this @ ");");%this.httpStatus="";%this.receiveText=false;%this.redirect=false;cancel(%this.retrySchedule);if(strLen(%this.request))%request=%this.request;else{%request=%this.buildRequest();}if($TCPClient::Debug)echo(">REQUEST:\n   >" SPC strReplace(%request,"\r\n","\n  >"));%this.send(%request);}function TCPClient::onDNSFailed(%this){%this.onDone($TCPClient::Error::dnsFailed);}function TCPClient::onConnectFailed(%this){if(%this.retryConnectionCount<$TCPClient::retryConnectionCount){%this.retryConnectionCount ++;if($TCPClient::PrintErrors){warn("WARN (TCPClient): Connection to server" SPC %this.server SPC "failed." SPC"Retrying in" SPC $TCPClient::retryConnectionWait @ "ms" SPC"(attempt" SPC %this.retryConnectionCount SPC "of" SPC $TCPClient::retryConnectionCount @ ")");}cancel(%this.retrySchedule);%this.retrySchedule=%this.schedule($TCPClient::retryConnectionWait,"connect",%this.server @ ":" @ %this.port);}else%this.onDone($TCPClient::Error::connectionFailed);}function TCPClient::onDisconnect(%this){%this.isConnected=false;if(!%this.redirect)%this.onDone($TCPClient::Error::none);}function TCPClient::onDone(%this,%error){if(%error && $TCPClient::PrintErrors){switch(%error){case $TCPClient::Error::dnsFailed: echo("\c2ERROR (TCPClient): error" SPC %error SPC "- DNS lookup failed.");case $TCPClient::Error::invalidResponse: echo("\c2ERROR (TCPClient): error" SPC %error SPC "- Server response:" SPC %this.httpStatus);case $TCPClient::Error::connectionTimedOut: echo("\c2ERROR (TCPClient): error" SPC %error SPC "- Connection timed out.");default: echo("\c2ERROR (TCPClient): error" SPC %error);}}if(isFunction(%this.className,"onDone"))eval(%this.className @ "::onDone(" @ %this @ ",\"" @ %error @ "\");");if(%this.isConnected){%this.disconnect();%this.isConnected=false;}cancel(%this.timeoutSchedule);%this.schedule(0,"delete");return %error;}function TCPClient::onLine(%this,%line){if($TCPClient::Debug)echo(">" @ %line);cancel(%this.timeoutSchedule);%this.timeoutSchedule=%this.schedule($TCPClient::timeout,"onDone",$TCPClient::Error::connectionTimedOut);if(%this.receiveText){%this.handleText(%line);}else{if(strLen(%line)){if(!%this.httpStatus && strStr(%line,"HTTP/")==0){%this.httpStatus=getWord(%line,1);%this.httpStatusDesc=getWord(%line,2);if(%this.httpStatus >= 400){%this.onDone($TCPClient::Error::invalidResponse);return;}else if(%this.httpStatus >= 300){if(%this.redirected){%this.onDone($TCPClient::Error::invalidRedirect);return;}else{%this.redirect=true;}}}else{%field=getWord(%line,0);%fieldLength=strLen(%field);if(%fieldLength>0){%field=getSubStr(%field,0,%fieldLength - 1);%value=getWords(%line,1);%this.headerField[%field]=%value;}switch$(%field){case "Location": if(%this.redirect){%this.disconnect();if(strStr(%value,"/")==0){%this.path=%value;}else{%pos=strStr(%value,"://");%url=getSubStr(%value,%pos + 3,strLen(%value));%pos=strStr(%url,"/");%this.server=getSubStr(%url,0,%pos) @ ":80";%this.path=getSubStr(%url,%pos,strLen(%url));}%this.redirected=true;%this.retrySchedule=%this.scheduleNoQuota($TCPClient::redirectWait,"connect",%this.server);return;}}}}else{%type=%this.headerField["Content-Type"];%savePathLen=strLen(%this.savePath);if(strLen(%type) && strStr(%type,"text/")==0 && !%savePathLen){%this.receiveText=true;}else{%this.setBinarySize(%this.headerField["Content-Length"]);}}}}function TCPClient::onBinChunk(%this,%chunk){if(isFunction(%this.className,"onBinChunk"))eval(%this.className @ "::onBinChunk(" @ %this @ ",\"" @ %chunk @ "\");");%contentLength=%this.headerField["Content-Length"];%contentLengthSet=(strLen(%contentLength)>0);if($TCPClient::Debug)echo(">" @ %chunk @ "/" @ %contentLength SPC "bytes");if(%contentLengthSet)%this.setProgressBar(%chunk / %contentLength);if(%chunk >= %contentLength && %contentLengthSet){%save=true;%done=true;}else{if(!%contentLengthSet){%save=true;%done=false;}cancel(%this.timeoutSchedule);%this.timeoutSchedule=%this.schedule($TCPClient::timeout,"onDone",$TCPClient::Error::connectionTimedOut);}if(%save){if(strLen(%this.savePath) && isWriteableFileName(%this.savePath)){%this.saveBufferToFile(%this.savePath);if(%done){%this.onDone($TCPClient::Error::none);}}else{%this.onDone($TCPClient::Error::invalidDownloadLocation);}}}function TCPClient::handleText(%this,%text){%text=expandEscape(%text);if(isFunction(%this.className,"handleText"))eval(%this.className @ "::handleText(" @ %this @ ",\"" @ %text @ "\");");}function TCPClient::setProgressBar(%this,%completed){if(isFunction(%this.className,"setProgressBar"))eval(%this.className @ "::setProgressBar(" @ %this @ ",\"" @ %completed @ "\");");}function urlGetComponents(%url){%pos=strStr(%url,"://");if(%pos==-1){%protocol="http";}else{%protocol=getSubStr(%url,0,%pos);%url=getSubStr(%url,%pos + 3,strLen(%url));}%pos=strStr(%url,"/");if(%pos==-1){%pos=strStr(%url,"?");if(%pos==-1){%server=%url;%path="/";}else{%server=getSubStr(%url,0,%pos);%path="/" @ getSubStr(%url,%pos,strLen(%url));}}else{%server=getSubStr(%url,0,%pos);%path=getSubStr(%url,%pos,strLen(%url));}%pos=strStr(%server,":");if(%pos==-1){switch$(%protocol){case "ftp": %port=21;case "telnet": %port=23;case "smtp": %port=25;case "nicname": %port=43;case "http": %port=80;case "https": %port=443;default: %port=80; }}else{%port=getSubStr(%server,%pos + 1,strLen(%server));%server=getSubStr(%server,0,%pos);}%pos=strStr(%path,"?");if(%pos != -1){%query=getSubStr(%path,%pos + 1,strLen(%path));%path=getSubStr(%path,0,%pos);}if(getSubStr(%path,strLen(%path) - 1,1) !$= "/" && strStr(%path,".")==-1)%path=%path @ "/";return %protocol TAB %server TAB %port TAB %path TAB %query;}
