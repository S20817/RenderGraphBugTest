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

## BugReport提出情報
### 対象シーン： BugTestScene1
- 不具合の再現手順: 
  - MainCameraに付いてる`BugTest1`コンポーネントの`RenderPassEvent`を`AfterRenderOpaques`~`AfterRenderingPostProcessing`の間の任意に設定
  - `BugTest1`の`UsingCopyPassStep1`と`UsingCopyPassStep1`のチェックをつける
  - MacOS Editor, Windows Editor, iOSビルド, Androidビルドで不具合再現（他のプラットフームは未確認）
- 実際の現象: 
  - `RenderPassEvent`が`AfterRenderOpaques`~`BeforeRenderingPostProcessing`の場合
    - 画面の大半がカメラのClearColorになり、正常描画時の色がモザイク状のノイズでフラッシュする
  - `RenderPassEvent`が`AfterRenderingPostProcessing`の場合
    - 画面の大半が真っ黒になり、正常描画時の色がモザイク状のノイズでフラッシュする
- 期待される動作
  - `BugTest1`の`UsingCopyPassStep1`と`UsingCopyPassStep1`のチェックを外した時と同じ描画結果

### 対象シーン： BugTestScene2
- 不具合の再現手順: 
  - MainCameraに付いてる`BugTest2`コンポーネントの`RenderPassEvent`を`AfterRenderingPostProcessing`に設定
  - `BugTest2`の`ConvertUsingFrameBufferFetch`と`RevertUsingFrameBufferFetch`のチェックをつける
  - MacOS Editor, Windows Editor, iOSビルド, Androidビルドで不具合再現（他のプラットフームは未確認）
- 実際の現象: 
  - CustomUI部分（青色の半透明板）以外、真っ黒になる（一部Android端末では、真っ黒ではなく、暗い赤色になる）
- 期待される動作
  - `BugTest2`の`ConvertUsingFrameBufferFetch`と`RevertUsingFrameBufferFetch`のチェックを外した時と同じ描画結果
