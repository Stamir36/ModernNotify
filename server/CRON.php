<?php
    // Unesell Studio - MN Connect - CNON task
    
    //Удалить все файлы в папке "files"
    if (file_exists('./files/')) {
        foreach (glob('./files/*') as $file) {
            unlink($file);
        }
        echo "Clear Files - Done.";
    }
    
?>