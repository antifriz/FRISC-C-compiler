<?php 
	$email = "";
	$title = "";
	$summary = "";
	if($_SERVER['REQUEST_METHOD'] == 'POST')
	{
		if(!array_key_exists('email', $_POST)
			|| !array_key_exists('title', $_POST)
			|| !array_key_exists('summary', $_POST))
			exit();
		$email = $_POST['email'];
		$title = $_POST['title'];
		$summary = $_POST['summary'];
		$contents = '<div>email="'.urlencode($email).'@fer.hr'.
							'"<br>title="'.urlencode($title).
							'"<br>summary="'.urlencode($summary).'"<br><br></div>';
		$filename = "./saved-data/feedback.html";
		file_put_contents($filename, $contents,FILE_APPEND);
		exit();
	}
?>
<!DOCTYPE html>
<html>
	<head>
		<meta charset="utf-8">
	</head>
	<body style="margin: auto;">
		<div style="margin-left: auto; margin-right: auto; width: 600px; display:block;">
			<form style="margin-top:25px;" name="forma" onsubmit="return validateForm();" method="post">
				<h2>
					Pošalji feedback:
				</h2>
				FER e-mail:<br>
				<input type="text" name="email" value="<?php echo $email; ?>" pattern="^[a-z0-9.]+$"/> @fer.hr<br><br>
				Naslov:<br>
				<input type="text" name="title" value="<?php echo $title; ?>"/><br><br>
				Tekst:<br>
				<textarea name="summary" rows="20" cols="80" style="resize: none;"><?php echo $summary; ?></textarea><br><br>
				<input type="submit" value="Pošalji" />
			</form>
		</div>
		<div align="right" style="position: fixed; right: 0;bottom:0;padding-bottom: 20px; padding-right: 40px;">Ivan Jurin, <a href="mailto:ivan.jurin@fer.hr">ivan.jurin@fer.hr</a></div>
		<div align="left" style="position: fixed; left: 0;bottom:0;padding-bottom: 20px; padding-left: 40px;">
			<a href="index.php" style="font-family: Calibri bold, Arial; font-size: large;">Compiler</a><br>
			<a href="fcc-main/data" style="font-family: Calibri bold, Arial; font-size: large;">Dostupne biblioteke i ugrađene funkcije</a><br>
			<a href="fcc-main/tst.c"  style="font-family: Calibri bold, Arial; font-size: large;">Testni primjer</a><br>
			<a href="feedback.php"  style="font-family: Calibri bold, Arial; font-size: large;">Feedback</a>
		</div>
		<script type="text/javascript">
			function validateForm () {
				var form = document.forms["forma"];
				return !(checkIfEmpty(form["email"], "Email") 
					|| checkIfEmpty(form["title"],"Title") 
					|| checkIfEmpty(form["summary"],"Textarea"));
			}
			function checkIfEmpty (text,stringName) {
				text = text.value;
				if (text==null || text=="")
				  {
				  	alert(stringName+" must be filled out");
				  	return true;
				  }
				return false;
			}
		</script>
	</body>
</html>