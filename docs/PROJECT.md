# Guia de manutenção

O repositório pode ser instalado diretamente como mod. A raiz contém o manifesto e os arquivos que o jogo procura na pasta `FantasticCreaturesPlus`.

| Caminho | Conteúdo |
| --- | --- |
| [`info.txt`](../info.txt) | Manifesto, arquivos de dados, imagens e módulo de história. |
| [`Events.xml`](../Events.xml) | Encontros, recompensas, crescimento e descendência. |
| [`DB/`](../DB/) | Raças, grupos, assentamentos, habilidades, cartas e equipamento. |
| [`DB/Lineages/`](../DB/Lineages/) | Fases, nomes e descrições das 64 combinações mestiças. |
| [`Images/`](../Images/) | Retratos por fase, arte de encontros e ícones. |
| [`Localization/Events.txt`](../Localization/Events.txt) | Textos de evento em português brasileiro. |
| [`docs/media/`](media/) | Capa do repositório e artes para publicação no Workshop. |
| [`docs/archive/`](archive/) | Notas históricas do desenvolvimento. |

## Validar alterações

Na raiz do repositório, execute com Python 3:

```sh
python tools/validate.py
```

O [validador](../tools/validate.py) confere caminhos do manifesto, IDs, referências entre bancos, retratos por fase e ligações entre nós de eventos. Use o [roteiro de testes em partida](PLAYTEST.md) para conferir o comportamento no jogo.

## Publicar

O GitHub oferece o ZIP da branch `main`. O [item do Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3838490787) tem publicação própria. A imagem `preview.jpg` na raiz é referenciada por `info.txt`. O jogo gerencia o arquivo local `modid.txt`, que fica fora do Git.
