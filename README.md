# Game-Webapp

Figma:
https://www.figma.com/design/JDzUs1guJwevl9QCegNvV6/Untitled?node-id=0-1&m=dev&t=EMIZ8sgbIOS1P64M-1


# OBLIG 1

## Hva prosjektet må inneholde
- CRUD på minst én entitet: Question er det naturlige valget. Lag sider for å opprette, vise, redigere og slette spørsmål (en slags "lærer/admin-side"). Da er kravet dekket uten at selve spillet må ha CRUD.

- Skjemaer og validering på serversiden: Skjemaet for å lage spørsmål (f.eks. [Required], [StringLength] og at riktig svar må være ett av alternativene), pluss skjemaet der spilleren skriver inn navnet sitt. Se Demo-ShopInputValidation.

- Feilhåndtering og logging: try/catch i repository/controller, ILogger som logger feil, og en egen feilside. Se Demo-ShopErrorHandlingLogging.

- Dynamisk innhold: Tilfeldige spørsmål fra databasen, fangede monstre og leaderboardet som oppdateres ut fra poeng.

- Design og navigasjon: En felles _Layout.cshtml med meny (Spill, Leaderboard, Mine monstre, Administrer spørsmål) og 2000-tallsstil med CSS.



## Oppgavefordeling
- Database: Modeller (Question, Monster, Player, CaughtMonster), DbContext, migrasjoner, seeding av spørsmål og monstre, repository-mønster. Følger ShopDatabase-demoene 1–6.

- Controllere og spill-logikk: QuizController (hent spørsmål, sjekk svar, gi monster), LeaderboardController, og at svaret sjekkes på serveren.

- CRUD-sidene og validering: QuestionController med Create/Edit/Delete-views og all server-side validering.

- Design og views: _Layout, CSS, pikselfont, monsterbilder og at spill-sidene ser ut som et gammelt nettspill.

- Kvalitet: Feilhåndtering og logging, kommentarer på engelsk, README og kildehenvisninger. Dette kan deles på flere, men én person bør ha ansvaret for at det faktisk blir gjort.


## Forslag til mappestruktur

MonsterQuiz/
├── Controllers/
│   ├── HomeController.cs          # Startside og feilside
│   ├── QuizController.cs          # Selve spillet: vis spørsmål, sjekk svar, gi monster
│   ├── QuestionController.cs      # CRUD for spørsmål (admin-sidene)
│   ├── LeaderboardController.cs   # Viser rangeringen
│   └── PlayerController.cs        # Skrive inn navn, se egne monstre
│
├── Models/
│   ├── Question.cs                # Spørsmål, alternativer, riktig svar, fag
│   ├── Monster.cs                 # Navn, bilde, sjeldenhet, poeng
│   ├── Player.cs                  # Spillerens navn
│   ├── CaughtMonster.cs           # Kobling: hvilken spiller fanget hvilket monster
│   └── ErrorViewModel.cs          # Brukes av feilsiden
│
├── ViewModels/
│   ├── QuizViewModel.cs           # Det spill-siden trenger (spørsmål UTEN fasit)
│   ├── AnswerResultViewModel.cs   # Riktig/feil + eventuelt fanget monster
│   └── LeaderboardViewModel.cs    # Liste med navn, poeng og plassering
│
├── DAL/
│   ├── QuizDbContext.cs           # Koblingen mellom C#-klassene og databasen
│   ├── DbInit.cs                  # Seeding: legger inn startspørsmål og monstre
│   ├── IQuestionRepository.cs     # "Kontrakt" for hva repositoryet kan gjøre
│   ├── QuestionRepository.cs      # Faktisk databasekode for spørsmål
│   ├── IPlayerRepository.cs
│   └── PlayerRepository.cs        # Spillere, fangede monstre og poeng
│
├── Migrations/                    # Genereres automatisk av EF (ikke rediger for hånd)
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml         # Felles ramme: meny, header, footer
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml           # Vennlig feilside
│   ├── Home/Index.cshtml
│   ├── Quiz/
│   │   ├── Play.cshtml            # Viser spørsmål og svaralternativer
│   │   └── Result.cshtml          # "Du fanget en Flammedrage!"
│   ├── Question/                  # Index, Create, Edit, Delete, Details
│   ├── Leaderboard/Index.cshtml
│   └── Player/
│       ├── Start.cshtml           # Skriv inn navnet ditt
│       └── MyMonsters.cshtml      # Samlingen din
│
├── wwwroot/
│   ├── css/site.css               # 2000-tallsstilen
│   ├── images/monsters/           # Monsterbildene
│   └── js/site.js
│
├── Program.cs                     # Oppstart: database, logging, ruter, feilhåndtering
├── appsettings.json               # Innstillinger, f.eks. connection string
└── README.md                      # Hvordan man starter appen + kildehenvisninger