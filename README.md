YouTube Search & Download 🎬🎵

Um programa de console em C# (.NET 8.0) para pesquisar e baixar vídeos ou áudios do YouTube usando o yt-dlp.
📋 Funcionalidades

    🔍 Pesquisa de vídeos no YouTube por palavra-chave

    📺 Exibe lista com os 10 primeiros resultados (título, duração e canal)

    🎬 Download de vídeo em formato MP4 (até 480p)

    🎵 Download de áudio em MP3 (e formatos alternativos: M4A, OPUS, WAV, AAC)

    🔄 Múltiplos métodos de download com fallback automático

    🛠️ Estratégias alternativas caso os métodos principais falhem

⚙️ Pré-requisitos

Antes de executar o programa, certifique-se de ter instalado:

    .NET 8.0 SDK ou superior

    yt-dlp - Adicione ao PATH do sistema

    FFmpeg - Necessário para conversão de áudio (MP3, etc.)

 -- Verificando as instalações

dotnet --version
yt-dlp --version
ffmpeg -version

🚀 Como usar

    Clone ou baixe o arquivo Program.cs

    Compile e execute:


dotnet run

Ou, se estiver em um projeto existente:
bash

dotnet build
dotnet run

    Siga as instruções no console:


=== YouTube Search & Download ===
🔍 Digite a frase-chave para pesquisar no YouTube: [digite sua pesquisa]

📺 Vídeos encontrados:
1. Título do vídeo
   Duração: 3:45 | Canal: Nome do Canal
...

🎯 Selecione o vídeo (1-10): [escolha o número]

📥 O que deseja baixar?
1. 🎬 Vídeo (MP4)
2. 🎵 Apenas Áudio (MP3)
Escolha uma opção (1-2): [escolha]

    O arquivo será salvo no diretório atual de execução.

📁 Estrutura do Projeto


Program.cs          # Código principal do programa
README.md           # Este arquivo

Classes principais

    Program - Classe principal com toda a lógica

    DownloadMethod - Representa um método de download (nome, comando, argumentos)

    YouTubeVideo - Representa um vídeo do YouTube (título, duração, canal, ID, URL)

🔧 Métodos de Download

O programa tenta automaticamente vários métodos em sequência:
Para Vídeo (MP4)

    yt-dlp com cookies do Chrome

    yt-dlp com headers customizados

    yt-dlp modo simples

    yt-dlp com extractor alternativo (Android/Web)

    yt-dlp formato MP4 genérico

Para Áudio (MP3)

    yt-dlp MP3 com cookies

    yt-dlp MP3 com headers customizados

    yt-dlp MP3 modo simples

    yt-dlp M4A

    yt-dlp OPUS

Se todos falharem, são tentadas estratégias alternativas (resoluções menores, formatos alternativos, etc.).
💡 Solução de Problemas

Se o download falhar, tente:

    Atualizar o yt-dlp:
    bash

    yt-dlp -U

    Verificar conexão com a internet

    Usar VPN (alguns vídeos podem estar bloqueados regionalmente)

    Verificar se o FFmpeg está instalado (necessário para conversão de áudio)

    Verificar espaço em disco disponível

    Tentar em outro horário (o YouTube pode limitar requisições)

⚠️ Aviso Legal

Este projeto é destinado apenas para uso educacional e pessoal. Respeite os termos de serviço do YouTube e os direitos autorais dos criadores de conteúdo. Não utilize esta ferramenta para distribuir conteúdo protegido por direitos autorais sem autorização.

📝 Licença

Este projeto é de uso livre para fins educacionais.

🤝 Contribuições

Sugestões e melhorias são bem-vindas! Sinta-se à vontade para abrir issues ou enviar pull requests.
