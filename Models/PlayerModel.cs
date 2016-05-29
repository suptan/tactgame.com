using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tactgame.com.Models
{
    [Serializable]
    public class PlayerModel
    {
        #region Declaration

        /// <summary>
        /// The identifier.
        /// </summary>
        [Required]
        [Display(Name = "id")]
        private int id;

        /// <summary>
        /// The player name.
        /// </summary>
        [Required]
        [Display(Name = "name")]
        private string name;

        /// <summary>
        /// The stock name.
        /// </summary>
        [Required]
        [Display(Name = "cash")]
        private decimal cash;

        /// <summary>
        /// The portfolio value.
        /// </summary>
        [Required]
        [Display(Name = "portfolio")]
        private decimal portfolio;

        /// <summary>
        /// The profit and loss of stock in portfolio.
        /// </summary>
        [Required]
        [Display(Name = "pl")]
        private decimal pl;

        /// <summary>
        /// The stock in player portfolio.
        /// </summary>
        private List<StockModel> stocks = new List<StockModel>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the player id.
        /// </summary>
        /// <value>The ID.</value>
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
        /// Gets or sets the player cash.
        /// </summary>
        /// <value>The cash.</value>
        public decimal Cash
        {
            get { return this.cash; }
            set { this.cash = value; }
        }

        /// <summary>
        /// Gets or sets the portfolio value.
        /// </summary>
        /// <value>The portfolio.</value>
        public decimal Portfolio
        {
            get { return this.portfolio; }
            set { this.portfolio = value; }
        }

        /// <summary>
        /// Gets or sets the portfolio profit and loss value.
        /// </summary>
        /// <value>The profit and loss.</value>
        public decimal PL
        {
            get { return this.pl; }
            set { this.pl = value; }
        }

        public List<StockModel> Stocks
        {
            get { return this.stocks; }
            set { this.stocks = value; }
        }

        #endregion

        public PlayerModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Player class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="cash">Cash.</param>
        /// <param name="portfolio">Portfolio.</param>
        public PlayerModel(int id, string name, decimal cash, decimal portfolio)
        {
            this.id = id;
            this.name = name;
            this.cash = cash;
            this.portfolio = portfolio;
        }
    }
}

