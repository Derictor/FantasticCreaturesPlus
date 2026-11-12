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
