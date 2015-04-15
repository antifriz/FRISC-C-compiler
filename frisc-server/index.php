
<?php
//phpinfo();
//pexit;
$wrongFileType = 0;
$alert = "";
$logEcho = "";

//if(!array_key_exists("stipe", $_GET)) return;

//echo var_dump($_FILES),var_dump($_POST),var_dump($_GET),"<script>alert(\"NOPOST. {$_SERVER["REQUEST_METHOD"]}\");</script>"; 
function compileFile()
{
    global $wrongFileType, $alert;
    if ($_SERVER["REQUEST_METHOD"] != "POST" || !array_key_exists("fileToCompile",$_FILES))
    {
       // $alert = "<script>alert(\"NOPOST. {$_SERVER["REQUEST_METHOD"]}\");</script>";
        return null;
    }
    $file = $_FILES["fileToCompile"];
    if ($file["error"] == UPLOAD_ERR_NO_FILE)
    {
        $alert = "<script>alert(\"UPLOAD_ERR_NO_FILE.\");</script>";
        return null;
    }

    $fileInfo = pathinfo($file["name"]);

    function checkFile($file, $fileInfo)
    {
        global $wrongFileType, $alert;
        $allowedTypes = array("text/plain", "text/x-c");
        $allowedExts = array("c", "C");
        $fileSizeLimit = 1000;

       // if (!in_array($file["type"], $allowedTypes))
        //{
          //  return UPLOAD_ERR_EXTENSION;
            //disallowed type
        //}
        if (!in_array($fileInfo["extension"], $allowedExts))
        {
            return UPLOAD_ERR_EXTENSION;
            //disallowed extension
        }
        if ($file["size"] > $fileSizeLimit)
        {
            return UPLOAD_ERR_FORM_SIZE;
            //file too large
        }
        return $file["error"];
    }

    if (($checkFileVal = checkFile($file, $fileInfo)) != null)
    {
        $alert = "<script>alert(\"Wrong file type.\");</script>";
        return null;
    }

   	$tmp_name = substr($file["tmp_name"],strlen("/tmp/"));
    move_uploaded_file($file["tmp_name"], "./tmp/".$tmp_name.".c");
	$command = "./script.sh ".$tmp_name;
	$process = proc_open($command, array(1=>array("pipe","w"),2=>array("pipe","w")), $pipes);
	$out = str_replace("pregcc.c",$file["name"],str_replace("\n","<br>","<br>".stream_get_contents($pipes[1])."<br>".stream_get_contents($pipes[2])));
	$val = proc_close($process);
    return array($val, $out, $fileInfo['filename'] . ".S",$tmp_name.".S");
}

$compilerOutBundle = compileFile();

if ($compilerOutBundle == null)
{
    //$logEcho = $compilerOutBundle[2];
}
elseif ($compilerOutBundle[0] == 0)
{
	$filename = "tmp/".$compilerOutBundle[3];
	if(file_exists($filename))
	{
	    $logEcho = file_get_contents($filename);

	    header("Content-Type: text/plain");
	    header('Content-Disposition: attachment; filename="' . $compilerOutBundle[2] . '"');
	    header("Content-Length: " . strlen($logEcho));
	  //  header("Location: " . $_SERVER['HTTP_REFERER']);
	    echo ";Compiled with FRISC C Compiler\n";
	    echo ";frisc.tel.fer.hr\n\n";
	    echo $logEcho;
	 	unlink($filename);
	    exit ;
	}
	$logEcho="Syntax check passed, compiling failed.<br>Check if all used libraries are included.";
}
else
    $logEcho = $compilerOutBundle[1];

?>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
	<head>
		<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
		<link rel="stylesheet" type="text/css" href="css/style.css">
  		<link rel="stylesheet" href="css/mainstyle.css" />
		<script type="text/javascript" src="js/behavioral.js"></script>
		<script type="text/javascript" src="js/si.files.js"></script>
	</head>
	<body class="bodyDiv" marginheight="0" marginwidth="0">
		<?php echo $alert; ?>
		<script>
		//	SI.Files.stylizeAll();
			function uploadFileTest() {
				var input, file;
				if (!window.FileReader) {
					alert("The file API isn't supported on this browser yet.");
					return;
				}

				//document.getElementById('downloadImg').setAttribute("src", "res/img/Download_img_watermark.png");
				//document.getElementById('compileImg').setAttribute("src", "res/img/Compile_img_watermark.png");
				//document.getElementById('compileImg').setAttribute("disabled", "disabled");

				input = document.getElementById('file');
				if (!input) {
					alert("Couldn't find the fileinput element.");
				} else if (!input.files) {
					alert("This browser doesn't seem to support the `files` property of file inputs.");
				} else if (!input.files[0]) {
					alert("Please select a file");
				} else {
					file = input.files[0];
					if (file.size > 2 * 1024 * 1024)
						alert("Please select file smaller than 2MB, size of this file is " + Math.round(file.size / 1024 / 1024 * 10) / 10 + "MB");
					else {
						var ext = file.name.split('.').pop();
						if (ext != "c" && ext != "C") {
							alert("Please select file with .c or .C extension.");
						} else {
							//document.getElementById('compileImg').removeAttribute("disabled");
							//document.getElementById('compileImg').setAttribute("src", "res/img/Compile_img.png");
							//var theDiv = document.getElementById("log");
                            //var content = document.createTextNode(file.name);
                            //theDiv.appendChild(content);
							var reader = new FileReader();
							reader.onloadend = function(evt) {
								if (evt.target.readyState == FileReader.DONE) {
									document.getElementById('textboxin').textContent = evt.target.result;
								}
							};
							reader.readAsBinaryString(file);
						}
					}
				}
			};
		</script>
		<div class="navigation">
	    	<a href="#">
	    		<img src="res/fcc.png" id="logo">
	    	</a>
	    </div>
	    <div class="container"
>	    	<div class="lijevo">
	    		<table>
					<a href="#" class="button">Upload</a>
					<a href="#" class="button">Compile</a>
				</table>
				<div class="fileUpload btn btn-primary">
					<span>Upload</span>
					<input type="file" id="file" name="fileToCompile" class="file" onchange="uploadFileTest();" />
				</div>
			</div>
			<div class="desno">
				<textarea id="textboxin" spellcheck="false" class="textboxl"></textarea>
				<textarea class="textboxd" spellcheck="false"></textarea>
			</div>
		</div>
		<div class="footer">
	  	</div>
<?php return; ?>

		<div align="center" style="height: 60px; background: #33B5E5; position:
		fixed; left: 0;right: 0;top:0;">
			<div align="left" style="width: 720px; padding-top: 22px;"><img
				src="res/img/FCC.png" />
			</div>
			<div align="right" style="position: fixed; right: 0;bottom:0;padding-bottom: 20px; padding-right: 40px;">Ivan Jurin, <a href="mailto:ivan.jurin@fer.hr">ivan.jurin@fer.hr</a></div>
			<div align="left" style="position: fixed; left: 0;bottom:0;padding-bottom: 20px; padding-left: 40px;">
				<a href="index.php" style="font-family: Calibri bold, Arial; font-size: large;">Compiler</a><br>
				<a href="fcc-main/data" style="font-family: Calibri bold, Arial; font-size: large;">Dostupne biblioteke i ugrađene funkcije</a><br>
				<a href="fcc-main/tst.c"  style="font-family: Calibri bold, Arial; font-size: large;">Testni primjer</a><br>
				<a href="feedback.php"  style="font-family: Calibri bold, Arial; font-size: large;">Feedback</a>
			</div>
		</div>
		<div style="height: 1px; background: #0099CC; position: fixed; left: 0;right:
		0;top:60px;">

		</div>
		<div style="height: 61px; background: #F8F8F8;">
		</div>
		<div align="center" style="background: #F8F8F8;">
			<form name="input" method="post" enctype="multipart/form-data">
				<table>
					<tr>
						<td style="padding: 20px;">
							<label class="cabinet">
								<input type="file" id="file" name="fileToCompile"  class="file" onchange="uploadFileTest();"/>
							</label> 
						</td>
						<td style="padding: 20px;">
						<input disabled="disabled" title="Compile uploaded file" id="compileImg" type="image" src="res/img/Compile_img_watermark.png" style="outline: none;" />
						</td>
						<!--<td style="padding: 20px;"> <img title="Download assembly code"
						id="downloadImg" src="res/img/Download_img_watermark.png"/> </td>-->
					</tr>
				</table>
			</form>
		</div>
		<div style="height: 1px;background: #DDDDDD;">
		</div>
		<div align="center">
			<div style="width: 720px; padding-top:10px; font-family: Calibri bold, Arial; font-size: x-large;" align="left">
				LOG:
			</div>
			<div style="width: 720px; font-size: large" align="left" id="log">
				<?php echo $logEcho;?>
			</div>
		</div>
	</body>
</html>

