namespace Extras
{
    public class Move
    {
        private int originSquare;
        private int targetSquare;
        private Piece pieceCaptured;

        public int GetOriginSquare()
        {
            return originSquare;
        }

        public void SetOriginSquare(int value)
        {
            originSquare = value;
        }
        
        public int GetTargetSquare()
        {
            return targetSquare;
        }

        public void SetTargetSquare(int value)
        {
            targetSquare = value;
        }

        public Piece GetPieceCaptured()
        {
            return pieceCaptured;
        }

        public void SetPieceCaptured(Piece value)
        {
            pieceCaptured = value;
        }
        
        public Move(int originSquare, int targetSquare, Piece pieceCaptured = null)
        {
            this.originSquare = originSquare;
            this.targetSquare = targetSquare;
            this.pieceCaptured = pieceCaptured;
        }
    }
}