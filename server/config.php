<?php

    ini_set('error_reporting', E_ALL);
    ini_set('display_errors', 1);
    ini_set('display_startup_errors', 1);

    header('Content-Type: application/json; charset=utf-8');

    $Host = 'localhost';
    $User = 'u409496471_mydevice';
    $Password = 'Stas1214';
    $Database = 'u409496471_mydevice';

    
    $mysql = new mysqli($Host, $User, $Password, $Database);

?>