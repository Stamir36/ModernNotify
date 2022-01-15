<?php
    include 'config.php';
    $id = $_GET["id"];

    $result = $mysql->query("DELETE FROM `mobile_device` WHERE `mobile_device`.`ID_PC` ='$id'");
    $device = $result->fetch_assoc();

    echo json_encode($device);  
    $mysql->close();  
?>