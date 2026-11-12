# Testes em partida

Use uma campanha descartável ou o editor de eventos do jogo para repetir os resultados.

| Cenário | Resultado esperado |
| --- | --- |
| Domar um adulto e acolher a criatura | Entra um personagem da espécie com equipamento padrão. |
| Domar um adulto e levar um pet | O inventário recebe um pet equipável. |
| Vencer Recrutar em um encontro de Filhote ou Jovem | Entra a fase do encontro, com o retrato e o contador correspondentes. |
| Esgotar o primeiro contador de uma criatura | O evento transforma o Filhote em Jovem e inicia um segundo contador. |
| Esgotar o segundo contador | O Jovem alcança o Auge, preserva o nível e deixa de ser filtrado como criança. Confira também equipamento e habilidades aprendidas. |
| Um adulto fantástico e um adulto de outra linhagem no mesmo grupo | O evento raro da combinação pode conceder uma criança mestiça. Repita com sexos iguais e diferentes. |
| O mesmo grupo com um Filhote ou Jovem no lugar do adulto fantástico | O evento da combinação fica indisponível. |
| Os dois participantes adultos em grupos separados | O evento da combinação fica indisponível. |
| Criança mestiça após os dois contadores | O retrato passa pelas duas fases juvenis e chega à arte própria da combinação no Auge. |

A chance de nascimento é de 1% por avaliação do evento. Cada combinação tem `allowOnce` e limita a dois o número de crianças já presentes no grupo. Para testes manuais de reprodução, rode uma cópia do módulo no editor com chance maior.

## Herança entre gerações

- **Humano + Orc** deve gerar uma criança com `Human Heritage` e `Orc Heritage`. Confira +2 de astúcia e −1 de misticismo da linhagem humana e +2 de força e −1 de inteligência da orc.
- **Orc–Anão + Elfo** deve gerar uma criança com três passivas, incluindo a proteção física e o custo de movimento anões. As passivas devem continuar após as duas transições e após salvar e recarregar.
- **Humano–Orc + Humano–Elfo** deve transmitir três linhagens distintas. A contribuição humana aparece uma vez.
- **Mestiços de saves anteriores** devem receber as passivas quando um evento reconhecer a sub-raça do mod ou os traços raciais conservados pelo jogo base.
- **Crianças, Jovens e adultos em grupos diferentes** ficam fora da seleção de pais.


## Modelos 3D no mapa

- Coloque uma criatura em um grupo pequeno em terra e mova o grupo. O jogo pode mostrar até três membros, conforme o poder. Repita com cada espécie.
- Compare Filhote, Jovem e Auge. Observe a escala, os chifres do Kirin e do Unicórnio, as asas do Grifo e da Fênix e a crista do Basilisco. Confira também os retratos da ficha.
- Verifique o movimento das patas dos quadrúpedes e do Ent, o bater de asas do Grifo e da Fênix e o movimento da cauda da Sereia e do Basilisco.
- O script usa Harmony quando a biblioteca está disponível na instalação. Se o carregador ou um modelo falhar, o jogo usa o modelo nativo de reserva daquela fase. Procure por [FCP] no log para distinguir os casos.


## Cooperativo com anfitrião e convidado

1. Em dois computadores, use a mesma versão do jogo, do mod e dos demais mods ativos, na mesma ordem. Abra uma campanha de teste e confirme que ambos entram na sala.
2. O convidado inicia um encontro com um adulto. O anfitrião participa do desafio como aliado. Repita com o anfitrião iniciando e compare as rotas de recrutar e de levar um pet. A recompensa deve cair apenas no grupo que iniciou.
3. Coloque filhotes ou jovens em grupos de ambos os jogadores. Quando os cronômetros vencerem, confira as transições e os traços hereditários de cada grupo. Adultos de grupos diferentes não devem formar pares.
4. Confirme que anfitrião e convidado veem os mesmos modelos nativos no mapa. Salve, feche a sala, carregue a campanha e reconecte o convidado.
