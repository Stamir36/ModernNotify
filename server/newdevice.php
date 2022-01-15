<?php
    
    include 'config.php';

    $ID_PC = $_GET["ID_PC"];
    $BATTETY = $_GET["BATTETY"];
    $MOBILE = $_GET["MOBILE"];

    $mysql->query("DELETE FROM `mobile_device` WHERE ID_PC = '$ID_PC';");
    $result = $mysql->query("INSERT INTO `mobile_device` (`ID`, `ID_PC`, `BATTETY`, `MOBILE`) VALUES (NULL, '$ID_PC', '$BATTETY', '$MOBILE')");

    $mysql->close();
    
?>