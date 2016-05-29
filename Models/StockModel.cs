using System;
using System.ComponentModel.DataAnnotations;

namespace tactgame.com.Models
{
    [Serializable]
    public class StockModel
    {
        #region Declaration

        /// <summary>
        /// The identifier.
        /// </summary>
        [Required]
        [Display(Name = "id")]
        private int id;

        /// <summary>
        /// The stock name.
        /// </summary>
        [Required]
        [Display(Name = "name")]
        private string name;

        /// <summary>
        /// The stock price.
        /// </summary>
        [Required]
        [Display(Name = "price")]
        private decimal price;

        /// <summary>
        /// The stock dividend.
        /// </summary>
        [Required]
        [Display(Name = "dividend")]
        private decimal dividend;

        /// <summary>
        /// The stock volumn.
        /// </summary>
        [Required]
        [Display(Name = "vol")]
        private int vol;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the stock id.
        /// </summary>
        /// <value>The I.</value>
        public int ID
        {
            get
            { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets the stock name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets the stock price.
        /// </summary>
        /// <value>The price.</value>
        public decimal Price
        {
            get { return this.price; }
            set { this.price = value; }
        }

        /// <summary>
        /// Gets or sets the stock dividend.
        /// </summary>
        /// <value>The dividend.</value>
        public decimal Dividend
        {
            get { return this.dividend; }
            set { this.dividend = value; }
        }

        /// <summary>
        /// Gets or sets the stock volumn.
        /// </summary>
        /// <value>The volumn.</value>
        public int Vol
        {
            get { return this.vol; }
            set { this.vol = value; }
        }

        #endregion

        public StockModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Stock class.
        /// </summary>
        /// <param name="name">Name.</param>
        public StockModel(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance of the Stock class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="price">Price.</param>
        /// <param name="dividend">Dividend.</param>
        public StockModel(int id, string name, decimal price, decimal dividend)
        {
            this.id = id;
            this.name = name;
            this.price = price;
            this.dividend = dividend;
        }

        /// <summary>
        /// Initializes a new instance of the Stock class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="price">Price.</param>
        /// <param name="dividend">Dividend.</param>
        /// <param name="vol">Volumn.</param>
        public StockModel(int id, string name, decimal price, decimal dividend, int vol)
        {
            this.id = id;
            this.name = name;
            this.price = price;
            this.dividend = dividend;
            this.vol = vol;
        }
    }
}

