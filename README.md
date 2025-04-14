# RenderGraphBugTest説明

## プロジェクト情報
- Unity Version: 6000.0.44f1
- URP Version: 17.0.4

## プロジェクト構成
- BugTestScene1: RenderGraph.AddCopyPassの不具合再現シーン
- BugTestScene2: UberPost直後にFrameBufferFetchの不具合再現シーン

### BugTestScene1
ActiveCameraColorAttachment -> TempTexture -> ActiveCameraColorAttachment

上記2回のコピーを行う

MainCameraに付いてる`BugTest1`コンポーネントで設定の切り替えが可能
- RenderPassEvent: 該当Passの実行タイミング
- UsingCopyPassStep1: 1回目のコピーにAddCopyPassを使う（チェック外すとAddBlitPassになる）
- UsingCopyPassStep1: 2回目のコピーにAddCopyPassを使う（チェック外すとAddBlitPassになる）

### BugTestScene２
ActiveCameraColorAttachment -sRGBに変換-> TempTexture -> TempTextureにCustomUI描画 -Linearに変換-> ActiveCameraColorAttachment

上記処理を行う

MainCameraに付いてる`BugTest2`コンポーネントで設定の切り替えが可能
- RenderPassEvent: 該当Passの実行タイミング（AfterRenderPostProcessing時に問題発生）
- ConvertUsingFrameBufferFetch: sRGB変換処理にFrameBufferFetchを使う（チェック外すと通常Blitになる）
- RevertUsingFrameBufferFetch: Linear変換処理時にFrameBufferFetchを使う（チェック外すと通常Blitになる）

### 補足
C#およびShaderコードに問題がある箇所に`Problem`のコメントを付けてるので、`Problem`で検索すればすぐに見つけられます。
