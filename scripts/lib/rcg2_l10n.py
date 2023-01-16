# This file is part of River City Girls L10n project
# Licensed under GPL-3+ License
# (c) 2020, 2023 Azamat H. Hackimov <azamat.hackimov@gmail.com>

from datetime import datetime
from enum import Enum
import csv
import logging
from os import makedirs
from os.path import join, exists
from polib import POEntry, POFile, pofile


class Rcg2CsvKeys(Enum):
    # Some constants from CSV dictionary
    # ACHIEVEMENTS = "Achievements"  # Empty category (for now)
    BAKED_TEXT = "Baked Text"
    CREDITS = "Credits"
    DIALOG = "Dialog"
    HONKR = "Honkr"
    HONKRNOSLANG = "HonkrNoSlang"
    MANGA = "Manga"
    QUESTS = "Quests"
    SHEET1 = "Sheet1"
    TICKER = "Ticker"
    # AUTODIALOG = "autoDialog"  # Seems unused, so we can safely exclude it


class Rcg2Languages(Enum):
    LANG_ENGLISH = {"key": "English", "iso_code": "en"}
    LANG_FRENCH = {"key": "French", "iso_code": "fr"}
    LANG_GERMAN = {"key": "German", "iso_code": "de"}
    LANG_ITALIAN = {"key": "Italian", "iso_code": "it"}
    LANG_SPANISH = {"key": "Spanish", "iso_code": "es"}
    LANG_JAPANESE = {"key": "Japanese", "iso_code": "ja"}
    LANG_KOREAN = {"key": "Korean", "iso_code": "ko"}
    LANG_CHINESESIMPLIFIED = {"key": "Chinese (Simplified)", "iso_code": "zh-cn"}
    LANG_CHINESETRADITIONAL = {"key": "Chinese (Traditional)", "iso_code": "zh-tw"}
    LANG_RUSSIAN = {"key": "Russian", "iso_code": "ru"}


LANG_KEY = "Key"

METADATA_ENTRY = {
    'Project-Id-Version': '1.0',
    'Report-Msgid-Bugs-To': 'you@example.com',
    'POT-Creation-Date': datetime.now().strftime("%Y-%m-%d %H:%M%z"),
    'PO-Revision-Date': datetime.now().strftime("%Y-%m-%d %H:%M%z"),
    'Last-Translator': 'you <you@example.com>',
    'Language-Team': '',
    'MIME-Version': '1.0',
    'Content-Type': 'text/plain; charset=UTF-8',
    'Content-Transfer-Encoding': '8bit',
}


class Rcg2Translation:
    def __init__(self, csv_path):
        """
        Init class for handling RCG_LocalizationData.json
        :param csv_path: Path to CSV file (UTF-8, ";" as delimiter)
        """
        self.logger = logging.getLogger("RCG2")
        with open(csv_path, "r", newline="", encoding="utf-8") as read_file:
            csv_content = csv.DictReader(read_file, delimiter=";")
            self.content = list(csv_content)

    def save_csv(self, csv_path, languages):
        """
        Save class as complete CSV file
        :param csv_path: path to CSV file
        :param languages: list of languages being processed into result CSV
        :return:
        """
        with open(csv_path, "w", encoding="utf-8", newline="") as write_file:
            fieldnames = ["Key", "Type", "Desc"]
            for i in languages:
                language = next(name.value["key"] for name in Rcg2Languages if name.value["iso_code"] == i)
                fieldnames.append(language)
            writer = csv.DictWriter(
                write_file,
                fieldnames=["Key", "Type", "Desc",
                            Rcg2Languages.LANG_ENGLISH.value["key"],
                            Rcg2Languages.LANG_FRENCH.value["key"],
                            Rcg2Languages.LANG_GERMAN.value["key"],
                            Rcg2Languages.LANG_ITALIAN.value["key"],
                            Rcg2Languages.LANG_SPANISH.value["key"],
                            Rcg2Languages.LANG_JAPANESE.value["key"],
                            Rcg2Languages.LANG_KOREAN.value["key"],
                            Rcg2Languages.LANG_CHINESESIMPLIFIED.value["key"],
                            Rcg2Languages.LANG_CHINESETRADITIONAL.value["key"],
                            Rcg2Languages.LANG_RUSSIAN.value["key"],
                            ], delimiter=";")
            writer.writeheader()
            for row in self.content:
                writer.writerow(row)
        return

    def load_po(self, path, csv_root_key, lang):
        """
        Load content of Gettext PO file (from "path/lang/json_root_key.po") into already loaded JSON content
        :param path: Root directory
        :param csv_root_key: JSON key from RcgJsonKeys class
        :param lang: Language. Should be in RcgLanguages class
        :return:
        """

        language = next(name.value["key"] for name in Rcg2Languages if name.value["iso_code"] == lang)

        po_file = join(path, lang, csv_root_key.value + ".po")

        if exists(po_file):
            po = pofile(po_file)

            for entry in po:
                csv_entry = next(item for item in self.content if item[LANG_KEY] == csv_root_key.value + "/" + entry.msgctxt)
                if not entry.obsolete and entry.translated() and "fuzzy" not in entry.flags:
                    csv_entry[language] = entry.msgstr
                else:
                    if csv_entry[language] == "":
                        # Add entry if there is no entry at all
                        csv_entry[language] = entry.msgid
        else:
            self.logger.warning("ERROR: '{}' is not exists! Skipping.".format(po_file))

        return

    def save_po(self, path, languages):
        """
        Save Gettext PO file into directory structure as "path/lang/csv_root_key.po"
        If "path/lang/json_root_key.po" already exists, it will be updated accordingly to CSV dict
        :param path: Root directory where place to files
        :param lang: Language to translate. Should be in Rcg2Languages class
        :return:
        """

        class PotFileEntry:
            def __init__(self, path, pot_file, create=False):
                self.path = path
                self.po_file = pot_file
                self.create = create

        # Create POT files
        pot_files = dict()
        pot_path = join(path, "pot")
        if not exists(pot_path):
            makedirs(pot_path)

        for i in Rcg2CsvKeys:
            pot_files[i.value] = PotFileEntry(join(pot_path, i.value + ".pot"), POFile(check_for_duplicates=True),
                                              not exists(join(pot_path, i.value + ".pot")))
            pot_files[i.value].po_file.metadata = METADATA_ENTRY
            pot_files[i.value].po_file.metadata_is_fuzzy = 1

        for row in self.content:
            (category, key) = row[LANG_KEY].split("/")
            if category in pot_files.keys() and key != "" and row[Rcg2Languages.LANG_ENGLISH.value["key"]] != "":
                po_entry = POEntry(
                    msgctxt=key,
                    msgid=row[Rcg2Languages.LANG_ENGLISH.value["key"]],
                )
                # Add translator's comment with japanese string
                po_entry.comment = row[Rcg2Languages.LANG_JAPANESE.value["key"]]
                try:
                    pot_files[category].po_file.append(po_entry)
                except ValueError:
                    self.logger.debug("Entry {} already exists, skipping...".format(row[LANG_KEY]))
        # We ready to dump POT files into FS
        for i in Rcg2CsvKeys:
            self.logger.info("Saving POT-file {}".format(pot_files[i.value].path))
            pot_files[i.value].po_file.save(pot_files[i.value].path)

        for lang_code in languages:
            language = next(name.value["key"] for name in Rcg2Languages if name.value["iso_code"] == lang_code)
            save_path = join(path, lang_code)
            if not exists(save_path):
                makedirs(save_path)

            for i in Rcg2CsvKeys:
                po_path = join(save_path, i.value + ".po")
                po_file = pot_files[i.value].po_file
                for entry in po_file:
                    target_str = self.find_csv_entry(language, "/".join([i.value, entry.msgctxt]))
                    if target_str != "":
                        entry.msgstr = target_str
                        entry.flags.append("fuzzy")
                if exists(po_path):
                    self.logger.info("Merging PO-file {}".format(po_path))
                    po = pofile(po_path)
                    po.merge(po_file)
                    po.save(po_path)
                else:
                    self.logger.info("Saving PO-file {}".format(po_path))
                    po_file.save(po_path)
        return

    def find_csv_entry(self, column, input):
        """
        Lookup entry in loaded CSV file
        :param column: Where to search
        :param input: Reference input in English
        :return: Translated string or empty string if no such entry
        """
        for row in self.content:
            if row[LANG_KEY] == input:
                return row[column]
        self.logger.warning("Entry {} ({}) not found in CSV".format(input, column))
        return ""
