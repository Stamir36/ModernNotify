<?php
    include 'config.php';
    $id_download = $_GET["id"];

    // Каталог, в который мы будем принимать файл:
    $uploaddir = './files/';
    $filenames = $id_download."_".basename($_FILES['file']['name']);
    
    $uploadfile = $uploaddir.$filenames;
    // Копируем файл из каталога для временного хранения файлов:
    if (copy($_FILES['file']['tmp_name'], $uploadfile))
    {
        echo "<h3>Файл успешно загружен на сервер</h3>";
    }
    else { echo "<h3>Ошибка! Не удалось загрузить файл на сервер!</h3>"; exit; }
    
    $filenames = "Download:".$id_download."_".basename($_FILES['file']['name']);
    
    $result = $mysql->query("UPDATE `pc_device` SET `command` = '$filenames' WHERE `ID_PC` = '$id_download'");
    $device = $result->fetch_assoc();
    $mysql->close();  
?>