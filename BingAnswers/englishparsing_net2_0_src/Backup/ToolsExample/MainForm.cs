using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;

namespace ToolsExample
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnParse;
		private System.Windows.Forms.Button btnPOSTag;
		private System.Windows.Forms.Button btnChunk;
		private System.Windows.Forms.Button btnTokenize;
		private System.Windows.Forms.Button btnNameFind;
		private System.Windows.Forms.TextBox txtOut;
		private System.Windows.Forms.Button btnSplit;
		private System.Windows.Forms.TextBox txtIn;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private string mModelPath;

		private OpenNLP.Tools.SentenceDetect.MaximumEntropySentenceDetector mSentenceDetector;
		private OpenNLP.Tools.Tokenize.EnglishMaximumEntropyTokenizer mTokenizer;
		private OpenNLP.Tools.PosTagger.EnglishMaximumEntropyPosTagger mPosTagger;
		private OpenNLP.Tools.Chunker.EnglishTreebankChunker mChunker;
        private OpenNLP.Tools.Parser.EnglishTreebankParser mParser;
		private OpenNLP.Tools.NameFind.EnglishNameFinder mNameFinder;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//mModelPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
			//mModelPath = new System.Uri(mModelPath).LocalPath + @"\Models\";
            mModelPath = @"C:\Projects\DotNet\OpenNLP\OpenNLP\Models\";
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnParse = new System.Windows.Forms.Button();
            this.btnPOSTag = new System.Windows.Forms.Button();
            this.btnChunk = new System.Windows.Forms.Button();
            this.btnTokenize = new System.Windows.Forms.Button();
            this.btnNameFind = new System.Windows.Forms.Button();
            this.txtOut = new System.Windows.Forms.TextBox();
            this.btnSplit = new System.Windows.Forms.Button();
            this.txtIn = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(360, 104);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(75, 23);
            this.btnParse.TabIndex = 21;
            this.btnParse.Text = "Parse";
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            // 
            // btnPOSTag
            // 
            this.btnPOSTag.Location = new System.Drawing.Point(184, 104);
            this.btnPOSTag.Name = "btnPOSTag";
            this.btnPOSTag.Size = new System.Drawing.Size(75, 23);
            this.btnPOSTag.TabIndex = 20;
            this.btnPOSTag.Text = "POS tag";
            this.btnPOSTag.Click += new System.EventHandler(this.btnPOSTag_Click);
            // 
            // btnChunk
            // 
            this.btnChunk.Location = new System.Drawing.Point(272, 104);
            this.btnChunk.Name = "btnChunk";
            this.btnChunk.Size = new System.Drawing.Size(75, 23);
            this.btnChunk.TabIndex = 19;
            this.btnChunk.Text = "Chunk";
            this.btnChunk.Click += new System.EventHandler(this.btnChunk_Click);
            // 
            // btnTokenize
            // 
            this.btnTokenize.Location = new System.Drawing.Point(96, 104);
            this.btnTokenize.Name = "btnTokenize";
            this.btnTokenize.Size = new System.Drawing.Size(75, 23);
            this.btnTokenize.TabIndex = 18;
            this.btnTokenize.Text = "Tokenize";
            this.btnTokenize.Click += new System.EventHandler(this.btnTokenize_Click);
            // 
            // btnNameFind
            // 
            this.btnNameFind.Location = new System.Drawing.Point(448, 104);
            this.btnNameFind.Name = "btnNameFind";
            this.btnNameFind.Size = new System.Drawing.Size(75, 23);
            this.btnNameFind.TabIndex = 16;
            this.btnNameFind.Text = "Find Names";
            this.btnNameFind.Click += new System.EventHandler(this.btnNameFind_Click);
            // 
            // txtOut
            // 
            this.txtOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOut.Location = new System.Drawing.Point(8, 136);
            this.txtOut.Multiline = true;
            this.txtOut.Name = "txtOut";
            this.txtOut.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOut.Size = new System.Drawing.Size(523, 400);
            this.txtOut.TabIndex = 15;
            this.txtOut.WordWrap = false;
            // 
            // btnSplit
            // 
            this.btnSplit.Location = new System.Drawing.Point(8, 104);
            this.btnSplit.Name = "btnSplit";
            this.btnSplit.Size = new System.Drawing.Size(75, 23);
            this.btnSplit.TabIndex = 14;
            this.btnSplit.Text = "Split";
            this.btnSplit.Click += new System.EventHandler(this.btnSplit_Click);
            // 
            // txtIn
            // 
            this.txtIn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIn.Location = new System.Drawing.Point(8, 8);
            this.txtIn.Multiline = true;
            this.txtIn.Name = "txtIn";
            this.txtIn.Size = new System.Drawing.Size(523, 88);
            this.txtIn.TabIndex = 13;
            this.txtIn.Text = resources.GetString("txtIn.Text");
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(539, 542);
            this.Controls.Add(this.btnParse);
            this.Controls.Add(this.btnPOSTag);
            this.Controls.Add(this.btnChunk);
            this.Controls.Add(this.btnTokenize);
            this.Controls.Add(this.btnNameFind);
            this.Controls.Add(this.txtOut);
            this.Controls.Add(this.btnSplit);
            this.Controls.Add(this.txtIn);
            this.Name = "MainForm";
            this.Text = "OpenNLP Tools Example";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		#region Button click events

		private void btnSplit_Click(object sender, System.EventArgs e)
		{		
			string[] sentences = SplitSentences(txtIn.Text);
			
			txtOut.Text = string.Join("\r\n\r\n", sentences);
		}

		private void btnTokenize_Click(object sender, System.EventArgs e)
		{
			StringBuilder output = new StringBuilder();

			string[] sentences = SplitSentences(txtIn.Text);

			foreach(string sentence in sentences)
			{
				string[] tokens = TokenizeSentence(sentence);
				output.Append(string.Join(" | ", tokens)).Append("\r\n\r\n");
			}

			txtOut.Text = output.ToString();
		}

		private void btnPOSTag_Click(object sender, System.EventArgs e)
		{
			StringBuilder output = new StringBuilder();

			string[] sentences = SplitSentences(txtIn.Text);

			foreach(string sentence in sentences)
			{
				string[] tokens = TokenizeSentence(sentence);
				string[] tags = PosTagTokens(tokens);

				for (int currentTag = 0; currentTag < tags.Length; currentTag++)
				{
					output.Append(tokens[currentTag]).Append("/").Append(tags[currentTag]).Append(" ");
				}

				output.Append("\r\n\r\n");
			}

			txtOut.Text = output.ToString();
		}

		private void btnChunk_Click(object sender, System.EventArgs e)
		{
			StringBuilder output = new StringBuilder();

			string[] sentences = SplitSentences(txtIn.Text);

			foreach(string sentence in sentences)
			{
				string[] tokens = TokenizeSentence(sentence);
				string[] tags = PosTagTokens(tokens);

				output.Append(ChunkSentence(tokens, tags)).Append("\r\n");
			}

			txtOut.Text = output.ToString();
		}

		private void btnParse_Click(object sender, System.EventArgs e)
		{
			StringBuilder output = new StringBuilder();

			string[] sentences = SplitSentences(txtIn.Text);

			foreach(string sentence in sentences)
			{
				output.Append(ParseSentence(sentence).Show()).Append("\r\n\r\n");
			}

			txtOut.Text = output.ToString();
		}

		private void btnNameFind_Click(object sender, System.EventArgs e)
		{
			StringBuilder output = new StringBuilder();

			string[] sentences = SplitSentences(txtIn.Text);

			foreach(string sentence in sentences)
			{
                output.Append(FindNames(sentence)).Append("\r\n");
			}

			txtOut.Text = output.ToString();
		}

		#endregion

		#region NLP methods

		private string[] SplitSentences(string paragraph)
		{
			if (mSentenceDetector == null)
			{
				mSentenceDetector = new OpenNLP.Tools.SentenceDetect.EnglishMaximumEntropySentenceDetector(mModelPath + "EnglishSD.nbin");
			}

			return mSentenceDetector.SentenceDetect(paragraph);
		}

		private string[] TokenizeSentence(string sentence)
		{
			if (mTokenizer == null)
			{
				mTokenizer = new OpenNLP.Tools.Tokenize.EnglishMaximumEntropyTokenizer(mModelPath + "EnglishTok.nbin");
			}

			return mTokenizer.Tokenize(sentence);
		}

		private string[] PosTagTokens(string[] tokens)
		{
			if (mPosTagger == null)
			{
				mPosTagger = new OpenNLP.Tools.PosTagger.EnglishMaximumEntropyPosTagger(mModelPath + "EnglishPOS.nbin", mModelPath + @"\Parser\tagdict");
			}

			return mPosTagger.Tag(tokens);
		}

		private string ChunkSentence(string[] tokens, string[] tags)
		{
			if (mChunker == null)
			{
				mChunker = new OpenNLP.Tools.Chunker.EnglishTreebankChunker(mModelPath + "EnglishChunk.nbin");
			}
			
			return mChunker.GetChunks(tokens, tags);
		}

		private OpenNLP.Tools.Parser.Parse ParseSentence(string sentence)
		{
			if (mParser == null)
			{
				mParser = new OpenNLP.Tools.Parser.EnglishTreebankParser(mModelPath, true, false);
			}

			return mParser.DoParse(sentence);
		}

		private string FindNames(string sentence)
		{
			if (mNameFinder == null)
			{
				mNameFinder = new OpenNLP.Tools.NameFind.EnglishNameFinder(mModelPath + "namefind\\");
			}

			string[] models = new string[] {"date", "location", "money", "organization", "percentage", "person", "time"};
			return mNameFinder.GetNames(models, sentence);
		}

		#endregion
		
	}
}
