<?php
    include 'config.php';
    $id = $_GET["id"];

    $result = $mysql->query("SELECT * FROM `pc_device` WHERE `ID_PC` = '$id'");
    $device = $result->fetch_assoc();

    echo json_encode($device);  
    
    $mysql->query(" UPDATE `pc_device` SET `command` = '----' WHERE `ID_PC` = '$id' ");
    $mysql->close();  
?>