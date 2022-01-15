<?php

    ini_set('error_reporting', E_ALL);
    ini_set('display_errors', 1);
    ini_set('display_startup_errors', 1);

    header('Content-Type: application/json; charset=utf-8');

    $Host = 'localhost';
    $User = 'id10230173_unesell';
    $Password = 'ModernNotify-Connect1';
    $Database = 'id10230173_device';

    
    $mysql = new mysqli($Host, $User, $Password, $Database);

?>