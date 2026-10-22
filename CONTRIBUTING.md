[Issue]:https://github.com/smdn/Smdn.Devices.Mcp2221A/issues/ "GitHub issues"
[Open an Issue]:https://github.com/smdn/Smdn.Devices.Mcp2221A/issues/new "GitHub issues"
[Pull Request]:https://github.com/smdn/Smdn.Devices.Mcp2221A/pulls/ "GitHub pull requests"
[General Bug Report]:https://github.com/smdn/Smdn.Devices.Mcp2221A/issues/new?template=01_bug-report.yml "GitHub issue template"
[Feature Request]:https://github.com/smdn/Smdn.Devices.Mcp2221A/issues/new?template=02_feature-request.yml "GitHub issue template"
[GitHub sponsors]:https://github.com/sponsors/smdn?frequency=one-time "GitHub sponsors"
[Smdn.MSBuild.ProjectAssets.Library]:https://github.com/smdn/Smdn.Fundamentals.BuildAssets "`Smdn.MSBuild.ProjectAssets.Library`"

# Contribution guidelines
Contributions are welcome! Submitting [Issue]s or [Pull Request]s is the primary way to contribute to this project.

Please refer to the relevant sections below based on the type of contribution.

[Issue]や[Pull Request]を送る際は、可能なら英語が望ましいですが、日本語で構いません。　詳細は以下の該当する項目に従ってください。

## How to contribute

### Reporting Issues / 不具合の報告
**Did you find a bug?** Please use the [General Bug Report] issue template to submit a bug report.

Completing the required fields in the template helps maintainers respond quickly and accurately.

### Feature Requests and Improvements / 機能追加・改善の提案
**Do you plan to add a new feature or improve an existing one?** Please use the [Feature Request] issue template.

Feel free to propose feature requests, API designs, or implementation improvements. Clear details about your goal and current limitations help facilitate smooth discussions.

### Contributing Changes to the Code / コードに対する変更
**Have you written a patch that adds a feature or fixes a bug?** Please submit a [Pull Request].

Including test cases for your changes is helpful, though not strictly required.

Small fixes and minor updates can be submitted directly via pull requests.

However, major changes such as those spanning multiple files should start as a [Feature Request] before submitting code.

### Contributing Changes to English Text / 英文に対する変更
**Did you find grammatical errors or awkward phrasing in the English text?** Please submit a [Pull Request].

Corrections from English speakers are welcome!

The use of machine translation may cause some English text to sound unnatural. Submitting a pull request to fix errors in user-facing content, such as documentation and code comments, is greatly appreciated.

機械翻訳を使用しているため、不自然な英語となっている場合があります。　ドキュメントやコードのコメントを含め、特にユーザーの目に触れる部分での英文の誤りを見つけた場合は、プルリクエスト等で修正の提案をいただけると助かります。

### Sponsor This Project / プロジェクトの支援
This project is open-source, and its artifacts are free to use under the terms of the project license. Your support through [GitHub sponsors] helps sustain continuous development and keeps the project motivated.

このプロジェクトはオープンソースであり、その成果物の利用はライセンスで定められる制限内において自由に行なえます。　継続的な開発のために、ぜひとも[GitHub sponsors]による支援をお願いします。

### Other Contributions / その他
For any other types of contributions, please [open an issue].

### ❌Unacceptable changes / 受け入れられない変更
Changes such as the following examples may not be acceptable:

- ❌Performance improvements without benchmarks.
  - ✅Including benchmark results in the pull request or discussing them in a [Feature Request] is required.
- ❌Changes that add dependencies, such as the use of third-party packages.
  - ✅Proposing external dependencies via a [Feature Request] first is required.
- ❌Changes using *tricky* or overly complex code (e.g., single-line hacks or code golf style).<br/>❌Code with unclear intent (e.g., excessively short variable names).
  - ✅Clear, self-documenting code is preferred over clever or compact code.



## Building and Testing / ビルドとテスト

### Prerequisites / 前提条件
Using the latest .NET SDK is recommended. Checking `global.json` in the repository root helps confirm the required SDK version.

できるだけ最新の.NET SDKの使用を推奨します。　必須となるSDKバージョンについては、リポジトリルートにある`global.json`を参照してください。

While the build process does not depend on IDE-specific tools, **Visual Studio Code** with the **C# Dev Kit** extension provides the recommended multi-platform development environment.

ビルドプロセスは特定のIDEに固有の機能・ツールには依存しませんが、OSを問わず使用できる環境として**Visual Studio Code + C# Dev Kit** の組み合わせを推奨します。

Refer to `README.md` for any additional repository-specific requirements.

その他、リポジトリ固有で必要となる要件がある場合は `README.md` を参照してください。

### Build and Test Procedures / ビルド・テスト手順
Running `dotnet build` or `dotnet test` at the repository root will build or test the entire solution when an `.slnx` file is present.

リポジトリルートに`.slnx`ファイルが存在する場合は、ルートディレクトリで`dotnet build`や`dotnet test`を実行することによりリポジトリ全体をビルド・テストできます。

Running `dotnet build` inside an individual project directory under `src/` (`src/<package-name>/*.csproj`) builds that specific assembly or package.

個別のパッケージやアセンブリをビルドする場合は、`src/`配下の各プロジェクトディレクトリ(`src/<package-name>/*.csproj`)で`dotnet build`を実行してください。

Running `dotnet test` inside a test project directory under `tests/` (`tests/<package-name>/*.Tests.csproj`) executes tests for that specific package.

個別のパッケージのテストを実行する場合は、`tests/`配下のテストプロジェクトディレクトリ(`tests/<package-name>/*.Tests.csproj`)で`dotnet test`を実行してください。

The `examples/` directory contains minimal code samples illustrating practical use cases. Refer to `README.md` inside `examples/` for execution details.

`examples/`ディレクトリが存在する場合、実際のユースケースを想定したミニマルなコードでの動作確認が可能です。　詳細は`examples/`配下の`README.md`を参照してください。

### Code Formatting and Static Analysis / コードフォーマット・静的解析
Shared build and test settings (`.props`/`.targets`) are provided by the SDK package [Smdn.MSBuild.ProjectAssets.Library].

ビルドやテストで用いられる共通設定(`.props`/`.targets`)は、SDK パッケージ[Smdn.MSBuild.ProjectAssets.Library]により提供されます。

Standard .NET SDK code analyzers apply to this project. Default analysis settings (such as `AnalysisMode` and `AnalysisLevel`) are configured automatically by the aforementioned package.

.NET SDK標準のコード解析が適用されます。　デフォルト設定(`AnalysisMode`や`AnalysisLevel`など)は上記のパッケージにより自動的に構成されます。

Executing `dotnet restore` automatically places the `.editorconfig` file, which defines the code style guidelines.

コードフォーマットを定める`.editorconfig`は、`dotnet restore`の実行時に自動的に配置されます。

Formatting violations appear as build warnings during `dotnet build`.

コードフォーマットへの違反がある場合は、`dotnet build` の実行時に警告として提示されます。

Resolving all formatting warnings before submitting a pull request is not strictly required. Unresolved formatting warnings can be adjusted by maintainers after merging.

フォーマット違反の警告をコントリビューター自身で完全に解消することは必須ではありません。　残った警告はPull Requestのマージ後にメンテナ側で修正します。

### Additional Documentation / その他のドキュメント
Refer to `README.md` in the repository root for any other repository-specific instructions regarding building, testing, or environment setup.

ビルド、テスト、および環境構築に関するその他の詳細やプロジェクト固有の事項については、リポジトリルートの `README.md` を参照してください。



## Requests when making changes to the code / コード変更時のお願い

### Code Formatting / コードフォーマット
Code formatting (indentation, line breaks, spacing, casing, etc.) should follow existing patterns and `.editorconfig` rules.

インデント・改行・空白・大文字小文字の規則などは、既存のコード、あるいは`.editorconfig`で定義されているフォーマットにならうようにしてください。

Minor formatting deviations are acceptable and can be adjusted during the review process.

フォーマットは多少逸脱していてもかまいません。　必要ならマージ前後で修正します。

### Test Code / テストコード
Adding test code that covers your changes or updating existing tests is greatly appreciated. While optional, test cases help verify the correctness and validity of the proposed changes. Sample code that demonstrates the behavior is also acceptable.

変更箇所をカバーするテストコードを提示いただくか、既存のテストへの追加をお願いします。　必須ではありませんが、変更内容の正確性・妥当性を検証するためのものでもあるので、可能な範囲でご協力ください。　変更結果を検証・実証するためのサンプルコードでも構いません。

### Copyright Notices / 著作権表示
Do not modify or add any copyright notices.

著作権表示を書き換えたり追加したりしないでください。

Upon merging pull requests, copyright notices will be updated as needed:

プルリクエストのマージ後、著作権表示は必要に応じて次のように更新されます。

```diff
 // SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
+// SPDX-FileCopyrightText: 202X smdn <smdn@smdn.jp> and contributors
 // SPDX-License-Identifier: MIT
```

### License Agreement / ライセンスへの同意
By submitting a pull request, all contributed changes are deemed to be licensed under the project's applicable license. While most projects are licensed under the MIT License, specific packages may use different licenses. Please refer to `LICENSE.txt` or `README.md` in the repository root, or the repository details on GitHub, to confirm the applicable license.

プルリクエストを送信することで、提出された変更内容がプロジェクトに適用されるライセンスの下で公開されることに同意したものとみなされます。　ほとんどのプロジェクトはMITライセンスの下で公開されていますが、パッケージ単位で異なるライセンスが適用されている場合もあります。　実際に適用されているライセンスについては、リポジトリルートの`LICENSE.txt`や `README.md`、またはGitHub上の記載を参照してください。



## Requests when writing and submitting in English
Project maintainers frequently rely on machine translation tools. Following these writing guidelines helps prevent misunderstandings:

- Please use conjunctive adverbs and linking words to clarify relationships between sentences. (ex: firstly, also, however, therefore, otherwise, on the other hand, by the way)
- Please avoid the use of abbreviations except technical terms.
  - Avoid jargons and non-technical abbreviation terms, as they may not be understood by unfamiliar people. (ex: WKM, FYI, LGTM, AFAIK, IMO)
  - You may use technical terms that are clear in context, or abbreviations that are redundant when written without abbreviations. (ex: IDE, API, HTTP)
  - If you are using an abbreviation for the first time in a sentence, it would be helpful if you could write the term without abbreviation in parentheses like this; PR (Pull Request), IV (initial vector), VSCode (Visual Studio Code).



## Response Times / 問い合わせ・レビューへの対応
Project maintainers handle reviews and issues in their spare time. While we aim to respond promptly, reviews may take some time. Thank you for your patience.

プロジェクトの管理・レビューは個人の時間で行われています。　できる限り速やかな対応を心がけていますが、確認までに時間を要する場合があります。　あらかじめご了承ください。
