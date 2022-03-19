<?php
    
    include 'config.php';

    $ID_PC = $_GET["ID_PC"];
    $BATTETY = $_GET["BATTETY"];
    $M1 = $_GET["M1"];
    $M2 = $_GET["M2"];
    $VOLUME = $_GET["VOLUME"];

    $c = $mysql->query("SELECT COUNT(*) AS COUNT FROM `pc_device` WHERE `ID_PC` = '$ID_PC'");
    $c_r = $c->fetch_assoc();
    $count = $c_r["COUNT"];
    
    if((int)$count > 0){
        echo "update";
        $result = $mysql->query("UPDATE `pc_device` SET `ID_PC` = '$ID_PC', `BATTETY` = '$BATTETY', `MusicTitle` = '$M1', `MusicAutor` = '$M2', `VOLUME` = '$VOLUME'  WHERE `ID_PC` = '$ID_PC'");
    }else{
        echo "add";
        $result = $mysql->query("INSERT INTO `pc_device` (`ID`, `ID_PC`, `BATTETY`, `MusicTitle`, `MusicAutor`, `VOLUME`) VALUES (NULL, '$ID_PC', '$BATTETY', '$M1', '$M2', '$VOLUME')");
    }

    $mysql->close();
    
?>