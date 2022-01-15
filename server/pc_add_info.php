<?php
    
    include 'config.php';

    $ID_PC = $_GET["ID_PC"];
    $BATTETY = $_GET["BATTETY"];
    $M1 = $_GET["M1"];
    $M2 = $_GET["M2"];
    $VOLUME = $_GET["VOLUME"];

    $mysql->query("DELETE FROM `pc_device` WHERE ID_PC = '$ID_PC';");
    $result = $mysql->query("INSERT INTO `pc_device` (`ID`, `ID_PC`, `BATTETY`, `MusicTitle`, `MusicAutor`, `VOLUME`) VALUES (NULL, '$ID_PC', '$BATTETY', '$M1', '$M2', '$VOLUME')");

    $mysql->close();
    
?>