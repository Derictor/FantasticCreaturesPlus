# Fantastic Creatures Plus

![Capa do Fantastic Creatures Plus](FantasticCreaturesPlus_Capa.png)

Mod de **Thea 2: The Shattering** que adiciona oito criaturas míticas, encontros no mapa, armas temáticas e descendência de linhagens mistas. Os nomes de personagens e habilidades são em inglês. Os textos dos encontros e as descrições do mod são em português do Brasil.

## Como jogar

| Conteúdo | Como obter |
| --- | --- |
| **Filhote** | Vença Domar ou Recrutar em um encontro de filhote. Algumas cartas iniciais também concedem filhotes. |
| **Jovem** | Recrute em um encontro de Jovem ou acompanhe o primeiro crescimento. |
| **Adulto / Auge** | Recrute no encontro adulto ou acompanhe o segundo crescimento. Adultos recrutados em encontros recebem o equipamento padrão da espécie. |
| **Pet equipável** | Vença Domar no encontro adulto e escolha levar o pet associado ao encontro. |
| **Mestiço** | Mantenha no mesmo grupo um adulto de uma espécie fantástica e um adulto de uma linhagem compatível. O evento raro acolhe a criança no grupo. |

As oito espécies são Grifo, Kirin, Sereia, Fênix, Unicórnio, Ent, Basilisco e Lobo Lunar. Cada fase juvenil tem um contador de **25–45 turnos** e um retrato próprio. Os Jovens desbloqueiam uma habilidade ativa e um despertar passivo. O Auge libera as habilidades de assinatura da espécie.

As 64 combinações de mestiços cobrem as oito espécies fantásticas e as linhagens humana, élfica, orc, anã, goblin, élfica sombria, demoníaca e bestial. A origem familiar define o retrato das fases juvenis. Cada combinação tem seu próprio retrato no Auge.

Os eventos do mod de nascimento aceitam adultos de qualquer sexo no mesmo grupo. Cada combinação pode acontecer uma vez e exige no máximo duas crianças no grupo. Filhotes e Jovens são excluídos dos pares. Duas criaturas adultas da mesma espécie fantástica também podem gerar um filhote da espécie.

## Instalação manual

1. Baixe a branch `main` pelo botão **Code → Download ZIP** no GitHub.
2. Extraia a pasta e renomeie-a para `FantasticCreaturesPlus`.
3. Coloque-a em `Thea2_Data/StreamingAssets/Mods/` na instalação do jogo. O arquivo `info.txt` deve ficar diretamente dentro de `Mods/FantasticCreaturesPlus/`.
4. Ative **Fantastic Creatures Plus** no menu de mods do jogo.

Se você tiver uma instalação local e outra pelo Workshop, habilite uma cópia por vez. O identificador local `modid.txt` é gerenciado pelo jogo.

## Estrutura do projeto

| Caminho | Conteúdo |
| --- | --- |
| [`info.txt`](info.txt) | Manifesto, bancos registrados e imagens. |
| [`Events.xml`](Events.xml) | Encontros, nascimentos e crescimento. |
| [`DB/`](DB/) | Raças, grupos, assentamentos, habilidades, cartas e equipamento. |
| [`DB/Lineages/`](DB/Lineages/) | As três fases dos mestiços, seus nomes e suas descrições. |
| [`Images/`](Images/) | Arte dos encontros, ícones e retratos organizados por fase. |
| [`Localization/`](Localization/) | Textos PT-BR do módulo. |
| [`docs/`](docs/) | Roteiro de teste e notas de manutenção. |

## Verificação

Execute `python tools/validate.py` na raiz para conferir o manifesto, os recursos e as referências dos eventos. A [lista de testes em partida](docs/PLAYTEST.md) cobre crescimento, recrutamento, pets e descendência.
