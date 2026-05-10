# Shrooman Tycoon (C# Console Game)

## Popis projektu
Shrooman Tycoon je textový management simulátor vyvíjený v jazyce C#. Hráč se ujímá role pěstitele rostlin a hub, buduje svou zahradu, zpracovává úrodu a snaží se dosáhnout maximální "Aury" a prestiže.

## Hlavní funkcionality
- **Systém pěstování:** Dynamická zahrada s různými druhy (5 rostlin, 3 houby) s odlišnou dobou růstu.
- **Ekonomika:** Nákup semínek, prodej úrody a vliv "Aury" na prodejní ceny.
- **Dílna (Workshop):** Možnost zpracovávat úrodu v kuchyni nebo laboratoři (odemykatelné upgrady).
- **Save/Load systém:** Ukládání postupu do JSON souboru pro možnost pokračování v rozehrané hře.
- **Prestige:** Reset herního světa pro odemčení vzácných druhů hub při zachování trvalých bonusů (Aura).

## Použité technologie
- **Jazyk:** C# (.NET 8.0/9.0)
- **Knihovny:** - `Spectre.Console` (pro pokročilé UI, barvy a tabulky)
  - `System.Text.Json` (pro ukládání dat)

## Časová náročnost
- Vývoj jádra hry: cca 16 hodin
- Implementace UI a designu: 10 hodin
- Ladění Save/Load systému a logiky: 3 hodiny
- Celkem: cca 29 hodin
