<?php
    include 'config.php';
    $id = $_GET["id"];
    $command = $_GET["command"];

    $result = $mysql->query(" UPDATE `mobile_device` SET `command` = '$command' WHERE `ID_PC` = '$id' ");

    $mysql->close();  
?>