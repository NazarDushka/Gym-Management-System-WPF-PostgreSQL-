-- 1. SŁOWNIKI I KATEGORIE

CREATE TABLE typy_stref (
    id serial PRIMARY KEY,
    nazwa varchar(50) NOT NULL UNIQUE
);

CREATE TABLE stanowiska (
    id serial PRIMARY KEY,
    nazwa varchar(100) NOT NULL UNIQUE,
    opis text
);

-- 2. KLIENCI I ABONAMENTY

CREATE TABLE klienci (
    id serial PRIMARY KEY,
    imie varchar(100) NOT NULL,
    nazwisko varchar(100) NOT NULL,
    telefon varchar(15),
    email varchar(255) NOT NULL UNIQUE,
    data_urodzenia date,
    is_archived boolean DEFAULT false,
    uwagi text,
    created_at timestamp DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE typy_abonamentow (
    id serial PRIMARY KEY,
    nazwa varchar(100) NOT NULL,
    opis text,
    cena_bazowa decimal(10,2) NOT NULL,
    okres_waznosci_dni integer NOT NULL,
    max_wejsc integer, -- NULL dla nielimitowanego dostępu
    dostep_basen boolean DEFAULT false,
    dostep_silownia boolean DEFAULT true,
    dostep_zajecia_grupowe boolean DEFAULT false
);

CREATE TABLE abonamenty_klientow (
    id serial PRIMARY KEY,
    klient_id integer NOT NULL REFERENCES klienci(id),
    typ_id integer NOT NULL REFERENCES typy_abonamentow(id),
    data_rozpoczecia date NOT NULL DEFAULT CURRENT_DATE,
    data_zakonczenia date NOT NULL, -- Obliczana przy wstawianiu (data_rozpoczecia + okres_waznosci)
    cena_zakupu decimal(10,2) NOT NULL, -- Ustalenie ceny w momencie sprzedaży
    created_at timestamp DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT check_dates CHECK (data_zakonczenia >= data_rozpoczecia)
);

-- 3. PERSONEL I WYNAGRODZENIA

CREATE TABLE pracownicy (
    id serial PRIMARY KEY,
    imie varchar(100) NOT NULL,
    nazwisko varchar(100) NOT NULL,
    telefon varchar(20),
    email varchar(255) NOT NULL UNIQUE,
    stanowisko_id integer REFERENCES stanowiska(id),
    data_zatrudnienia date NOT NULL DEFAULT CURRENT_DATE,
    data_zwolnienia date,
    is_archived boolean DEFAULT false,
    CONSTRAINT check_employment_dates CHECK (data_zwolnienia IS NULL OR data_zwolnienia >= data_zatrudnienia)
);

CREATE TABLE dane_kadrowe (
    pracownik_id integer PRIMARY KEY REFERENCES pracownicy(id),
    pesel char(11) UNIQUE,
    numer_konta char(26),
    wynagrodzenie_godzinowe decimal(10,2),
    wynagrodzenie_miesieczne decimal(10,2),
    forma_zatrudnienia varchar(50)
);

-- 4. WIZYTY I CZAS PRACY

CREATE TABLE wejscia_klientow (
    id serial PRIMARY KEY,
    klient_id integer NOT NULL REFERENCES klienci(id),
    abonament_id integer NOT NULL REFERENCES abonamenty_klientow(id),
    data_wejscia timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_wyjscia timestamp,
    typ_wejscia varchar(20),
    CONSTRAINT check_visit_times CHECK (data_wyjscia IS NULL OR data_wyjscia >= data_wejscia)
);

CREATE TABLE rejestr_czasu_pracy (
    id serial PRIMARY KEY,
    pracownik_id integer NOT NULL REFERENCES pracownicy(id),
    godzina_rozpoczecia timestamp NOT NULL,
    godzina_zakonczenia timestamp,
    typ_czasu varchar(20), -- np. 'praca', 'urlop'
    zatwierdzone boolean DEFAULT false,
    zatwierdzil_id integer REFERENCES pracownicy(id),
    data_zatwierdzenia timestamp
);

-- 5. INFRASTRUKTURA I ZAJĘCIA

CREATE TABLE strefy (
    id serial PRIMARY KEY,
    nazwa varchar(100) NOT NULL,
    typ_strefy_id integer REFERENCES typy_stref(id),
    powierzchnia_m2 decimal(10,2),
    is_active boolean DEFAULT true
);

CREATE TABLE typy_zajec (
    id serial PRIMARY KEY,
    nazwa varchar(100) NOT NULL,
    opis text,
    czas_trwania_min integer NOT NULL,
    max_uczestnikow integer NOT NULL
);

CREATE TABLE zajecia_grupowe (
    id serial PRIMARY KEY,
    typ_zajec_id integer REFERENCES typy_zajec(id),
    strefa_id integer REFERENCES strefy(id),
    instruktor_id integer REFERENCES pracownicy(id),
    data_rozpoczecia timestamp NOT NULL,
    status varchar(20) DEFAULT 'zaplanowane'
);

CREATE TABLE zapisy_na_zajecia (
    id serial PRIMARY KEY,
    zajecia_id integer REFERENCES zajecia_grupowe(id) ON DELETE CASCADE,
    klient_id integer REFERENCES klienci(id) ON DELETE CASCADE,
    data_zapisu timestamp DEFAULT CURRENT_TIMESTAMP,
    status varchar(20) DEFAULT 'potwierdzone'
);

-- 1. Ograniczenia finansowe
ALTER TABLE typy_abonamentow ADD CONSTRAINT check_cena_bazowa_positive CHECK (cena_bazowa >= 0);
ALTER TABLE typy_abonamentow ADD CONSTRAINT check_okres_waznosci_positive CHECK (okres_waznosci_dni > 0);
ALTER TABLE typy_abonamentow ADD CONSTRAINT check_max_wejsc_positive CHECK (max_wejsc IS NULL OR max_wejsc > 0);

ALTER TABLE abonamenty_klientow ADD CONSTRAINT check_cena_zakupu_positive CHECK (cena_zakupu >= 0);

ALTER TABLE dane_kadrowe 
ALTER COLUMN wynagrodzenie_godzinowe SET NOT NULL;

ALTER TABLE dane_kadrowe 
ADD CONSTRAINT check_wynagrodzenie_positive 
CHECK (wynagrodzenie_godzinowe > 0);

-- 2. Logiczne ograniczenia dat
ALTER TABLE klienci ADD CONSTRAINT check_data_urodzenia CHECK (data_urodzenia < CURRENT_DATE AND data_urodzenia > '1900-01-01');
ALTER TABLE rejestr_czasu_pracy ADD CONSTRAINT check_work_time CHECK (godzina_zakonczenia IS NULL OR godzina_zakonczenia >= godzina_rozpoczecia);

-- 3. Walidacja formatów (Regex)
ALTER TABLE klienci ADD CONSTRAINT check_email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');
ALTER TABLE pracownicy ADD CONSTRAINT check_email_format_p CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');

ALTER TABLE klienci 
ADD CONSTRAINT check_telefon_format CHECK (telefon ~ '^[0-9]{9}$');

ALTER TABLE pracownicy 
ADD CONSTRAINT check_telefon_format_p CHECK (telefon ~ '^[0-9]{9}$');

ALTER TABLE dane_kadrowe ADD CONSTRAINT check_pesel_format CHECK (pesel ~ '^[0-9]{11}$' OR pesel IS NULL);
ALTER TABLE dane_kadrowe ADD CONSTRAINT check_nr_konta_format CHECK (numer_konta ~ '^[0-9]{26}$' OR numer_konta IS NULL);

-- 4. Ograniczenia statusów
ALTER TABLE zajecia_grupowe ADD CONSTRAINT check_status_zajecia CHECK (status IN ('zaplanowane', 'odwolane', 'zakonczone', 'w_trakcie'));
ALTER TABLE rejestr_czasu_pracy ADD CONSTRAINT check_typ_czasu CHECK (typ_czasu IN ('praca', 'urlop', 'chorobowe', 'delegacja'));

-- 5. Ograniczenia infrastrukturalne
ALTER TABLE strefy ADD CONSTRAINT check_powierzchnia_positive CHECK (powierzchnia_m2 > 0 OR powierzchnia_m2 IS NULL);
ALTER TABLE typy_zajec ADD CONSTRAINT check_max_uczestnikow_positive CHECK (max_uczestnikow > 0);

-- Ograniczenia statusów zapisów
ALTER TABLE zapisy_na_zajecia 
ADD CONSTRAINT check_status_zapisu CHECK (status IN ('potwierdzone', 'anulowane', 'lista_rezerwowa', 'obecny'));

-- Zakaz duplikatów
ALTER TABLE zapisy_na_zajecia 
ADD CONSTRAINT unique_klient_zajecia UNIQUE (klient_id, zajecia_id);

-- Sprawdzenie czasu trwania wizyty
ALTER TABLE wejscia_klientow 
ADD CONSTRAINT check_visit_duration CHECK (data_wyjscia IS NULL OR data_wyjscia >= data_wejscia);

-- Czas trwania zajęć
ALTER TABLE typy_zajec 
ADD CONSTRAINT check_duration_logic CHECK (czas_trwania_min BETWEEN 15 AND 300);

-- Widok dla podsumowania aktywnych klientów
CREATE VIEW podsumowanie_aktywnych_klientow AS
SELECT 
    k.id,
    k.imie,
    k.nazwisko,
    k.email,
    COUNT(wk.id) as liczba_wizyt
FROM klienci k
LEFT JOIN abonamenty_klientow ak ON k.id = ak.klient_id
LEFT JOIN wejscia_klientow wk ON ak.id = wk.abonament_id
WHERE k.is_archived = false
AND ak.data_zakonczenia >= CURRENT_DATE
GROUP BY k.id, k.imie, k.nazwisko, k.email;

/*
OPIS WIDOKU v_wyplaty_pracownikow:

FUNKCJE UŻYTE:
- EXTRACT(EPOCH FROM...) - pobiera różnicę czasową w sekundach
- SUM() - sumuje wartości
- / 3600 - przelicza sekundy na godziny
- ::decimal(10,2) - rzutuje wynik na typ dziesiętny z 2 miejscami po przecinku
*/

-- Widok do obliczania wypłat pracowników
CREATE OR REPLACE VIEW v_wyplaty_pracownikow AS
SELECT 
    p.id as pracownik_id,
    p.imie, 
    p.nazwisko,
    dk.wynagrodzenie_godzinowe,
    -- Obliczanie sumy godzin za bieżący miesiąc
    SUM(EXTRACT(EPOCH FROM (rcp.godzina_zakonczenia - rcp.godzina_rozpoczecia)) / 3600)::decimal(10,2) as suma_godzin,
    -- Obliczanie wynagrodzenia
    (SUM(EXTRACT(EPOCH FROM (rcp.godzina_zakonczenia - rcp.godzina_rozpoczecia)) / 3600) * dk.wynagrodzenie_godzinowe)::decimal(10,2) as naleznosc
FROM pracownicy p
JOIN dane_kadrowe dk ON p.id = dk.pracownik_id
JOIN rejestr_czasu_pracy rcp ON p.id = rcp.pracownik_id
WHERE rcp.godzina_zakonczenia IS NOT NULL 
  AND rcp.zatwierdzone = true
GROUP BY p.id, p.imie, p.nazwisko, dk.wynagrodzenie_godzinowe;
