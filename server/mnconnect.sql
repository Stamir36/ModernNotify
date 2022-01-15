-- phpMyAdmin SQL Dump
-- version 4.9.5
-- https://www.phpmyadmin.net/
--
-- Хост: localhost:3306
-- Время создания: Янв 14 2022 г., 20:44
-- Версия сервера: 10.5.12-MariaDB
-- Версия PHP: 7.3.32

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `id10230173_device`
--

-- --------------------------------------------------------

--
-- Структура таблицы `mobile_device`
--

CREATE TABLE `mobile_device` (
  `ID` int(255) NOT NULL,
  `ID_PC` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  `BATTETY` int(255) NOT NULL,
  `MOBILE` varchar(255) COLLATE utf8_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

--
-- Дамп данных таблицы `mobile_device`
--

INSERT INTO `mobile_device` (`ID`, `ID_PC`, `BATTETY`, `MOBILE`) VALUES
(1464, 'NBHG211001943150F17600', 38, 'M2101K7AG'),
(1942, 'PF2CTH9B', 57, 'M2101K9AG'),
(1943, 'PF2CTH9B', 57, 'M2101K9AG');

-- --------------------------------------------------------

--
-- Структура таблицы `pc_device`
--

CREATE TABLE `pc_device` (
  `ID` int(255) NOT NULL,
  `ID_PC` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  `BATTETY` int(11) NOT NULL,
  `VOLUME` int(11) NOT NULL,
  `MusicTitle` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  `MusicAutor` varchar(255) COLLATE utf8_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

--
-- Дамп данных таблицы `pc_device`
--

INSERT INTO `pc_device` (`ID`, `ID_PC`, `BATTETY`, `VOLUME`, `MusicTitle`, `MusicAutor`) VALUES
(10983, 'NBHG211001943150F17600', 100, 0, 'Музыка', 'Очередь_пуста.'),
(33797, 'PF2CTH9B', 75, 100, 'Sweet_Harmony_(Nu_Disco_Radio_Edit)', 'Block_');

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `mobile_device`
--
ALTER TABLE `mobile_device`
  ADD PRIMARY KEY (`ID`);

--
-- Индексы таблицы `pc_device`
--
ALTER TABLE `pc_device`
  ADD PRIMARY KEY (`ID`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `mobile_device`
--
ALTER TABLE `mobile_device`
  MODIFY `ID` int(255) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=1944;

--
-- AUTO_INCREMENT для таблицы `pc_device`
--
ALTER TABLE `pc_device`
  MODIFY `ID` int(255) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=33798;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
