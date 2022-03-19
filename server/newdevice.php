<?php
    
    include 'config.php';

    $ID_PC = $_GET["ID_PC"];
    $BATTETY = $_GET["BATTETY"];
    $MOBILE = $_GET["MOBILE"];
    $MEM1 = $_GET["MEM1"];
    $MEM2 = $_GET["MEM2"];

    $c = $mysql->query("SELECT COUNT(*) AS COUNT FROM `mobile_device` WHERE `ID_PC` = '$ID_PC'");
    $c_r = $c->fetch_assoc();
    $count = $c_r["COUNT"];
    
    if((int)$count > 0){
        echo "update";
        $result = $mysql->query("UPDATE `mobile_device` SET `ID_PC` = '$ID_PC', `BATTETY` = '$BATTETY', `MOBILE` = '$MOBILE', `MEM1` = '$MEM1', `MEM2` = '$MEM2'  WHERE `ID_PC` = '$ID_PC'");
    }else{
        echo "add";
        $result = $mysql->query("INSERT INTO `mobile_device` (`ID`, `ID_PC`, `BATTETY`, `MOBILE`, `MEM1`, `MEM2`) VALUES (NULL, '$ID_PC', '$BATTETY', '$MOBILE', '$MEM1', '$MEM2')");
    }
        
    
    //$mysql->query("DELETE FROM `mobile_device` WHERE ID_PC = '$ID_PC';");
    

    $mysql->close();
    
?>